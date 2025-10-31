using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using game;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Linq;

/* 
   //little anims on the sprites when there are move happenung
   challlenge modes
   tuto

   bugs:
   // multi target => screenshake revient pas
   /  les previews de leurs attaques sont pt
   les boutons des moves de l'ennemi selctione marchnet pas (le sprite de select est par desssus)
   // tuer pendant qu'il arrive
   ia comprends plus les buffs (pb avec attEfx (le Pop() ?))

   opti:
   // IA lente du cul (pickMove on dirait)
   render : plusieurs canvas - now l'ecran s'eteint des fois
*/

public sealed class combatManager : MonoBehaviour 
{
    private struct renderElements
    {
        public numBar health, energie;
        public TextMeshProUGUI shieldsTxt, name;
        public Image shieldImg, img;
        public RectTransform effect, move, movePreview;

        public renderElements(numBar health, numBar energie, TextMeshProUGUI shieldsTxt, Image shieldImg, TextMeshProUGUI name, Image img, RectTransform effect, RectTransform move, RectTransform movePreview)
        {
            this.health = health;
            this.energie = energie;
            this.shieldsTxt = shieldsTxt;
            this.name = name;
            this.shieldImg = shieldImg;
            this.img = img;
            this.effect = effect;
            this.move = move;
            this.movePreview = movePreview;
        }
    }
    public static GameObject statusEffectPrefab, playerStatsPrefab, enemyStatsPrefab, enemyMovePreviewsPrefab, enemyMovePreviewsTimesPrefab, numAnimPrefab, sfxPrefab, deathPrefab;
    public static Dictionary<int, enemy> current;
    public static List<int> ded;
    private static Dictionary<int, renderElements> enemyUIs;

    public static player player;
    private static renderElements playerUI;

    public static int combats, choiceCount;

    public static new RectTransform transform;
    public static RectTransform cursor;
    public static Transform canvas;
    public static Transform sfxTransform;

    public static int playerHealth, playerEnergy, enemyHealth, enemyEnergy;

    public static List<enemy> allEnemies;

    public static Dictionary<string, moveUpgrade> moveUpgrades;

    public static Dictionary<string, AudioClip> sounds;

    public static int target;

    public AudioSource muse;
    public static AudioSource music;

    public static bool canMove, gamer;

    public static EventSystem eventSystem;

    public int combatsAmount;

    public void Start() {
        transform = GetComponent<RectTransform>();
        cursor = transform.parent.Find("cursor") as RectTransform;

        statusEffectPrefab = Resources.Load<GameObject>("prefabs/statusEffect");
        playerStatsPrefab = Resources.Load<GameObject>("prefabs/playerStats");
        enemyStatsPrefab = Resources.Load<GameObject>("prefabs/enemyStats");
        numAnimPrefab = Resources.Load<GameObject>("prefabs/num");
        sfxPrefab = Resources.Load<GameObject>("prefabs/sound");
        deathPrefab = Resources.Load<GameObject>("prefabs/death");
        enemyMovePreviewsPrefab = Resources.Load<GameObject>("prefabs/enemyMovePreview");
        enemyMovePreviewsTimesPrefab = Resources.Load<GameObject>("prefabs/enemyMovePreviewTimes");

        sfxTransform = gameObject.scene.GetRootGameObjects()[0].transform;

        allEnemies = new List<enemy>(Resources.LoadAll<enemy>("enemies"));

        moveUpgrades = new Dictionary<string, moveUpgrade>();
        foreach (moveUpgrade m in Resources.LoadAll<moveUpgrade>("move upgrades"))
            moveUpgrades.Add(m.Name, m);
            
        enemy.enemyMoves = new List<move>();

        sounds = new Dictionary<string, AudioClip>();
        foreach (AudioClip a in Resources.LoadAll<AudioClip>("sounds"))
            sounds.Add(a.name, a); 

        music = muse;

        eventSystem = gameObject.scene.GetRootGameObjects()[2].GetComponent<EventSystem>();
        canvas = gameObject.scene.GetRootGameObjects()[3].transform.GetChild(0);
        ScreenShakerUI.Setup(canvas);

        StartCoroutine(next());
    }

    public static void Reset() {
        SceneManager.LoadScene("TitleScene");

        player = null;
        current = new ();
        ded = new ();
        playerHealth = playerEnergy = enemyEnergy = enemyHealth = 0;
        RemoveTarget();
        gamer = false;
        combats = choiceCount = 0;
    }

    public IEnumerator next() {
        if (player is null) {
            player = ScriptableObject.CreateInstance<player>();
            player.Moves = new List<move>();
            player.health = 50;
        }

        combatMenuManager.DisplayPlayer(player.Moves);

        if (choiceCount < 3) {
            generateOptions();
            yield break;
        }

        player.statusEffects = new List<statusEffect>();
        foreach (move m in player.Moves)
            m.reset();

        player.energy = player.maxEnergy = 18 + playerEnergy;
        player.maxHealth = 50 + playerHealth;
        player.block = 0;

        current = new ();
        ded = new ();
        
        if (combats == 0)
            addEnemy(Resources.Load<enemy>("enemies/tardivlad"));
        else if (combats % combatsAmount != 0) 
        {
            int r = combats + (gamer? 2 : 0);

            var possible = allEnemies.FindAll(E => (gamer || E.rarity * 2 <= combats +  2) && E.rarity >= 0).Where(x => x.rarity <= r).OrderByDescending(x => x.rarity);
            int len = possible.Count();
            int separator = Mathf.CeilToInt(len / 3f);
            int sum = 0;

            var beeg = possible.Take(separator);
            int beegLen = separator;
            if (beegLen > 0)
            {
                for (int i = 0; i < combats % 8; i++)
                {
                    var e = beeg.pickRandom();
                    r -= (e.rarity + 1);
                    addEnemy(e, sum ++);
                }
            }

            var smol = possible.Skip(separator).Where(x => x.rarity <= r);
            int smolLen = len - separator;
            while (r > 1) 
            {
                var e = smol.pickRandom(smolLen);
                r -= e.rarity;
                smolLen --;
                smol = smol.Except(new []{e}).Where(x => x.rarity <= r);
                addEnemy(e, sum ++);
            }
        } 
        else 
        {
            if (gamer)
                addEnemy(Resources.Load<enemy>("enemies/Herve"));
            else
                addEnemy(Resources.Load<enemy>("enemies/tardichad"));

            music.clip = Resources.Load<AudioClip>("music/boss");
            music.Play();
        }

        combats ++;

        yield return new WaitForSeconds(0.02f);

        Transform[] array = transform.GetComponentsInChildren<Transform>();
        for (int i = 1; i < array.Length; i++)
            Destroy(array[i].gameObject);

        GameObject g = Instantiate(playerStatsPrefab, new Vector3(), Quaternion.identity, transform);
        g.transform.localPosition = new Vector3(-199.39f, -122, 0);
        Graphic[] gs = g.GetComponentsInChildren<Graphic>();
        numBar[] bars = g.GetComponentsInChildren<numBar>();
        playerUI = new renderElements(
            bars[0],
            bars[1],
            Array.Find(gs, I => I.name == "shieldTxt") as TextMeshProUGUI,
            Array.Find(gs, I => I.name == "shieldImg") as Image,
            Array.Find(gs, g => g.name == "name") as TextMeshProUGUI,
            Array.Find(gs, g => g.name == "Img") as Image,
            g.transform.Find("effects") as RectTransform,
            g.transform.Find("moves") as RectTransform,
            g.transform.Find("movePreviews") as RectTransform
        );
        
        enemyUIs = new ();
        int count = 0;
        foreach (var a in current) {
            g = Instantiate(enemyStatsPrefab, new Vector3(), Quaternion.identity, transform);
            g.transform.localPosition = new Vector3(-transform.rect.width / 2 + (enemyStatsPrefab.GetComponent<RectTransform>().rect.width / 2) * (count * 1.2f + .7f), transform.rect.height / 2 - ((enemyStatsPrefab.GetComponent<RectTransform>().rect.height) / 4) * 1.2f);
            RightClick r = g.GetComponentInChildren<RightClick>();
            r.leftClick.AddListener(() => clicked(a.Key));
            r.hoverStart.AddListener(() => seeMoves(a));
            r.hoverEnd.AddListener(() => hideMoves(a.Key));
            r.displayType = RightClick.DisplayType.none;

            StartCoroutine(arrive(g.transform));

            gs = g.GetComponentsInChildren<Graphic>();
            bars = g.GetComponentsInChildren<numBar>();
            enemyUIs.Add(a.Key, new renderElements(
                bars[0],
                bars[1],
                Array.Find(gs, I => I.name == "shieldTxt") as TextMeshProUGUI,
                Array.Find(gs, I => I.name == "shieldImg") as Image,
                Array.Find(gs, g => g.name == "name") as TextMeshProUGUI,
                Array.Find(gs, g => g.name == "Img") as Image,
                g.transform.Find("effects") as RectTransform,
                g.transform.Find("moves") as RectTransform,
                g.transform.Find("movePreviews") as RectTransform
            ));

            count ++;
                        
            a.Value.calculateNextMoves();
        }

        combatMenuManager.removal = false;
        canMove = true;

        transform.parent.Find("end turn").GetComponent<Button>().interactable = true;

        updateSprites();

        target = 0;

        yield return new WaitForSeconds(1.5f);

        SetTarget(current.Keys.Min());

        static void addEnemy(enemy en, int i = 0)
        {
            var e = ScriptableObject.CreateInstance<enemy>();
            e.setTo(en);
            current.Add(i, e);
        }
    }

    public void generateOptions() {
        choiceCount ++;
        canMove = false;
        transform.parent.Find("end turn").GetComponent<Button>().interactable = false;

        eventSystem.SetSelectedGameObject(null);

        Transform option1 = transform.parent.Find("option 1"), option2 = transform.parent.Find("option 2");
        option1.gameObject.SetActive(false);
        option2.gameObject.SetActive(false);

        move m1, m2;

        bool isValidPair(move M1, move M2) => 
            M1.name != M2.name && M1.rarity == M2.rarity;

        List<(move, move)> getAllPossibilities() {
            List<(move, move)> result = new List<(move, move)>();

            int rarity = combats % 2 == 0? 1 : extensions.Rand<int>(Enumerable.Range(2, 3).ToArray(), Enumerable.Range(2, 3).Select(x => (x * (combats / (float)combatsAmount)) / x).ToArray());

            foreach (move m in move.allMoves.Where(m => m.rarity == rarity))
                foreach (move m2 in move.allMoves)
                    if (m != m2 && isValidPair(m, m2)) {
                        move _m = m, _m2 = m2;
                        while (player.Moves.Find(M => M.name == _m.name && M.Name.AmountOf('+') >= _m.Name.AmountOf('+')) || 
                            enemy.enemyMoves.Find(M => M.name == _m.name && M.Name.AmountOf('+') >= _m.Name.AmountOf('+')))
                            _m = _m.upgrade();

                        while (player.Moves.Find(M => M.name == _m2.name && M.Name.AmountOf('+') >= _m2.Name.AmountOf('+')) || 
                            enemy.enemyMoves.Find(M => M.name == _m2.name && M.Name.AmountOf('+') >= _m2.Name.AmountOf('+')))
                            _m2 = _m2.upgrade();

                        result.Add((_m, _m2));
                    }
            
            return result;
        }

        (move, move) getRandomPossibility()
        {
            List<(move, move)> possible = getAllPossibilities();
            int id = UnityEngine.Random.Range(0, possible.Count);
            return (possible[id].Item1, possible[id].Item2);
        }
            

        if (combats % 5 == 0 && combats != 0) 
        {
            var nametxt = option1.Find("name").GetComponent<TextMeshProUGUI>();
            nametxt.text = "max energy";
            nametxt.color = new Color(.3294118f, .5294118f, 1, 1);

            option1.Find("description").GetComponent<TextMeshProUGUI>().text = "+2 max energy";
            option1.Find("attackType").GetComponent<Image>().sprite = Resources.Load<Sprite>("icons/energy");
            option1.Find("energyImg").gameObject.SetActive(false);
            var button = option1.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(increaseMaxEnergy);
            button.OnDeselect(new BaseEventData(EventSystem.current));

            var nametxt2 = option2.Find("name").GetComponent<TextMeshProUGUI>();
            nametxt2.text = "max health";
            nametxt2.color = new Color(.7490196f, .3098039f, .3333333f, 1);
            option2.Find("description").GetComponent<TextMeshProUGUI>().text = "+4 max health";
            option2.Find("attackType").GetComponent<Image>().sprite = Resources.Load<Sprite>("icons/health");
            option2.Find("energyImg").gameObject.SetActive(false);
            var button2 = option2.GetComponent<Button>();
            button2.onClick.RemoveAllListeners();
            button2.onClick.AddListener(increaseMaxEnergy);
            button2.OnDeselect(new BaseEventData(EventSystem.current));
        } 
        else
        {
            (m1, m2) = getRandomPossibility();

            var nametxt = option1.Find("name").GetComponent<TextMeshProUGUI>();
            nametxt.text = m1.Name;
            nametxt.color = Color.white;

            option1.Find("description").GetComponent<TextMeshProUGUI>().text = m1.description;
            option1.Find("attackType").GetComponent<Image>().sprite = move.typeImages[m1.type];
            option1.Find("energyImg").gameObject.SetActive(true);
            option1.Find("energyImg/energyCost").GetComponent<TextMeshProUGUI>().text = m1.energyCost.ToString();
            var button = option1.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => pick(m1, m2));
            button.OnDeselect(new BaseEventData(EventSystem.current));

            var nametxt2 = option2.Find("name").GetComponent<TextMeshProUGUI>();
            nametxt2.text = m2.Name;
            nametxt2.color = Color.white;
            option2.Find("description").GetComponent<TextMeshProUGUI>().text = m2.description;
            option2.Find("attackType").GetComponent<Image>().sprite = move.typeImages[m2.type];
            option2.Find("energyImg").gameObject.SetActive(true);
            option2.Find("energyImg/energyCost").GetComponent<TextMeshProUGUI>().text = m2.energyCost.ToString();
            var button2 = option2.GetComponent<Button>();
            button2.onClick.RemoveAllListeners();
            button2.onClick.AddListener(() => pick(m2, m1));
            button2.OnDeselect(new BaseEventData(EventSystem.current));

            combatMenuManager.removal = true;
            combatMenuManager.DisplayPlayer(player.Moves);
        }

        StartCoroutine(appear(option1));
        StartCoroutine(appear(option2));
    }
    
    public IEnumerator appear(Transform t) {
        yield return new WaitForSeconds(.5f);

        t.gameObject.SetActive(true);

        for (int i = 0; i < 51; i++) {
            foreach (Graphic g in t.GetComponentsInChildren<Graphic>())
                g.color = new Color(g.color.r, g.color.g, g.color.b, i / 50f);

            yield return new WaitForSeconds(.02f);
        }
    }

    public IEnumerator disappear(Transform t, Vector3 dir) {
        Vector3 v = t.localPosition;
        
        for (int i = 50; i >= 0; i--) 
        {
            t.localPosition += dir * i * (3 / 5f);

            yield return new WaitForSeconds(.02f);
        }

        t.localPosition = v;
        t.gameObject.SetActive(false);
    }

    public IEnumerator arrive(Transform t) {
        Vector3 v = t.localPosition;
        t.localPosition = new Vector3(600, t.localPosition.y, t.localPosition.z);
        
        for (int i = 0; i < 51; i++) 
        {
            if (t is null) 
                break;

            t.localPosition += new Vector3((v.x - t.localPosition.x) / 10, 0, 0);

            yield return new WaitForSeconds(.02f);
        }

        if (t is not null)
            t.localPosition = v;
    }

    public IEnumerator dis(Vector3 dir1, Vector3 dir2) {
        StartCoroutine(disappear(transform.parent.Find("option 1"), dir1));
        yield return StartCoroutine(disappear(transform.parent.Find("option 2"), dir2));

        StartCoroutine(next());
    }

    public void pick(move m, move left) {
        if (player.Moves.Count > 12) 
            return;

        move p = ScriptableObject.CreateInstance<move>();
        p.setTo(m);
        move M;
        if ((M = player.Moves.Find(n => n.name == m.name)) is not null)
            player.Moves.Remove(M);
        player.Moves.Add(p);

        move e = ScriptableObject.CreateInstance<move>();
        e.setTo(left);
        if ((M = enemy.enemyMoves.Find(n => n.name == m.name)) is not null)
            enemy.enemyMoves.Remove(M);
        enemy.enemyMoves.Add(e);

        StartCoroutine(dis(transform.parent.Find("option 1/name").GetComponent<TextMeshProUGUI>().text == p.Name? Vector3.down : Vector3.up, transform.parent.Find("option 2/name").GetComponent<TextMeshProUGUI>().text == p.Name? Vector3.down : Vector3.up));
    }

    public static void removeMove(move m) {
        player.Moves.Remove(m);

        combatMenuManager.DisplayPlayer(player.Moves);
    }

    public void increaseMaxHealth() {
        playerHealth += 4;
        player.health += 4;
        enemyEnergy += 2;

        StartCoroutine(dis(Vector3.up, Vector3.down));
    }

    public void increaseMaxEnergy() {
        playerEnergy += 2;
        enemyHealth += 4;

        StartCoroutine(dis(Vector3.down, Vector3.up));
    }

    public static void Anim(string type, actor a, int val) {
        RectTransform r = Instantiate(numAnimPrefab, new Vector3(), Quaternion.identity, transform.parent).GetComponent<RectTransform>();
        AudioSource source = Instantiate(sfxPrefab, sfxTransform).GetComponent<AudioSource>();
        
        void _anim(renderElements e, Color c, float size)
        {
            r.position = e.shieldsTxt.transform.position;
            r.sizeDelta = e.shieldsTxt.rectTransform.sizeDelta * size;
            var t = r.GetComponent<TextMeshProUGUI>();
            t.color = c;
            t.text = val.ToString();
            sfx(type);
        }

        Color c = type switch
        {
            "block"  => new Color(.7686275f, .7686275f, .7686275f),
            "attack" => new Color(.7490196f, .3098039f, .3333333f),
            "heal"   => new Color(.6666667f, .8823529f, .4039216f),
            _ => Color.black
        };
        
        if (a is enemy e)
            _anim(enemyUIs[current.KeyOf(e)], c, .5f);
        else
            _anim(playerUI, c, .5f);
    }

    public static void sfx(string s) {
        AudioSource source = Instantiate(sfxPrefab, sfxTransform).GetComponent<AudioSource>();
        source.clip = sounds[s];
        source.Play(); 
    }

    public static void clicked(int i) {
        SetTarget(i);
    }

    public static void used(move m, actor a) {
        if (!canMove)
            return;

        if (target != -1 || m.appliesTo == move.AppliesTo.all)
            a.useMove(m, current[target]);

        if (current[target].statusEffects.Exists(e => e is shock))
            current[target].calculateNextMoves();

        updateSprites();
    }

    public void endTurn() {
        player.OnEndTurn();

        StartCoroutine(turn());
    }

    private IEnumerator turn() {
        canMove = false;

        updateSprites();
       
        yield return new WaitForSeconds(.1f);

        foreach (var I in current.Keys) 
        {
            var a = current[I];
            a.OnStartTurn(I);
            
            if (ded.Contains(I))
                goto cont;
                
            updateSprites();

            yield return new WaitForSeconds(.1f);

            for (int i = 0; i < a.nextMoves.Count; i++) {
                a.useMove(a.nextMoves[i], player);

                updateSprites();
                if (ded.Contains(I))
                    goto cont;

                yield return new WaitForSeconds(.5f / Mathf.Max(1, i - 2));
            }

            a.OnEndTurn();
            
            cont : 
            updateSprites();

            yield return new WaitForSeconds(.1f);
        }

        foreach (var enemy in current.Keys)
            if (!ded.Contains(enemy))
                current[enemy].calculateNextMoves();

        player.OnStartTurn(-1);
        
        canMove = true;

        updateSprites();
    }

    public static void seeMoves(KeyValuePair<int, enemy> a) {
        Transform[] array1 = enemyUIs[a.Key].move.GetComponentsInChildren<Transform>();
        for (int i2 = 1; i2 < array1.Length; i2++)
            Destroy(array1[i2].gameObject);
        for (int j = 0; j < a.Value.Moves.Count; j++) {
            float yPos = enemyUIs[a.Key].move.rect.height / 2 - (combatMenuManager.moveDisplayPrefab.GetComponent<RectTransform>().rect.height * (j + .5f));
            moveDisplay m = Instantiate(combatMenuManager.moveDisplayPrefab, new Vector3(), Quaternion.identity, enemyUIs[a.Key].move).GetComponent<moveDisplay>();
            m.GetComponentInChildren<RightClick>().leftClick.AddListener(() => SetTarget(a.Key));
            m.transform.localPosition = new Vector3(0, yPos, 0);
            m.move = a.Value.Moves[j];
            m.user = a.Value;
        }

        foreach (Graphic img in enemyUIs[a.Key].movePreview.GetComponentsInChildren<Graphic>())
            img.enabled = false;
    }

    public void hideMoves(int i) {
        if (i == -1 || ded.Contains(i) || !current.ContainsKey(i))
            return;

        Transform[] array1 = enemyUIs[i].move.GetComponentsInChildren<Transform>();
        for (int i2 = 1; i2 < array1.Length; i2++)
            Destroy(array1[i2].gameObject);

        foreach (Graphic g in enemyUIs[i].movePreview.GetComponentsInChildren<Graphic>())
            g.enabled = true;
    }

    public void Update() {
        if (!canMove)
            return;
        
        if (current.Count == 0 && !transform.parent.Find("option 1").gameObject.activeSelf) {
            //sfx("win");
            
            if (combats > combatsAmount)
                SceneManager.LoadScene("credits");
            else
                generateOptions();
        }
    }

    public static void SetTarget(int e)
    {
        if (!cursor.gameObject.activeSelf)
            cursor.gameObject.SetActive(true);

        target = e;
        RectTransform t = enemyUIs[e].move.parent as RectTransform;

        cursor.position = t.position + Vector3.forward * 5;
        cursor.sizeDelta = t.sizeDelta / 2 * 1.1f;
    }
    public static void RemoveTarget()
    {
        cursor.gameObject.SetActive(false);

        target = -1;
    }

    public static void updateSprites() {
        if (player is null)
            return;

        combatMenuManager.DisplayPlayer(player.Moves);

        if (canMove) 
        {
            var toRemove = new List<int>();
            int j = 0;
            foreach (int i in current.Keys) 
            {
                actor a = current[i];
                if (!ded.Contains(i))
                    continue;

                Instantiate(deathPrefab, enemyUIs[i].img.transform.position, Quaternion.identity, transform.parent);
                sfx("boom");
               
                toRemove.Add(i);

                if (target == j) {
                    if (current.Count - toRemove.Count != 0)
                        SetTarget(current.Keys.Min());
                    else 
                        RemoveTarget();
                }
            }
            foreach (var i in toRemove)
            {
                current.Remove(i);
                Destroy(enemyUIs[i].health.transform.parent.parent.gameObject);
                enemyUIs.Remove(i);
            }

            ded.Clear();
        }

        void draw(renderElements R, actor a)
        {
            R.health.var1 = a.health;
            R.health.var2 = a.maxHealth;
            R.energie.var1 = a.energy;
            R.energie.var2 = a.maxEnergy;
            R.shieldsTxt.text = a.block.ToString();
            R.shieldsTxt.enabled = R.shieldImg.enabled = a.block != 0;

            Transform[] array = R.effect.GetComponentsInChildren<Transform>();
            for (int i2 = 1; i2 < array.Length; i2++)
                Destroy(array[i2].gameObject);

            for (int j = 0; j < a.statusEffects.Count; j++) {
                statusEffect s = a.statusEffects[j];
                float xPos = -R.effect.rect.width / 2 + (statusEffectPrefab.GetComponent<RectTransform>().rect.width / 2) * (j * 2 + 1);
                GameObject g = Instantiate(statusEffectPrefab, new Vector3(), Quaternion.identity, R.effect);
                g.transform.localPosition = new Vector3(xPos, 0, 0);
                RightClick r = g.GetComponentInChildren<RightClick>();
                r.displayType = RightClick.DisplayType.statusEffect;
                r.statusEffectDisplay = a.statusEffects[j];
                g.GetComponentInChildren<Image>().sprite = s.sprite;
                g.GetComponentInChildren<TextMeshProUGUI>().text = s.value.ToString();
            }
        }

        draw(playerUI, player);
        
        for (int i = 0; i < current.Count; i++) {
            draw(enemyUIs[i], current[i]);
            
            enemyUIs[i].name.text = current[i].Name;
            enemyUIs[i].img.sprite = current[i].sprite;

            Transform[] array1 = enemyUIs[i].movePreview.GetComponentsInChildren<Transform>();
            for (int i2 = 1; i2 < array1.Length; i2++)
                Destroy(array1[i2].gameObject);                

            int amount = 0, difference = 0;
            List<move> nextMoves = (current[i] as enemy).nextMoves;
            
            if (nextMoves.Count == 0) 
                continue;

            //move prevMove = nextMoves[0];
            for (int j = 0; j < nextMoves.Count; j ++) {
                /*if (prevMove == nextMoves[j]) {
                    amount ++;

                    if (j != nextMoves.Count - 1)
                        continue;
                } else {
                    prevMove = nextMoves[j];

                    j --;
                }*/
                while (j < nextMoves.Count-1 && nextMoves[j] == nextMoves[j+1])
                {
                    amount ++;
                    j ++;
                }
                
                float xPos = enemyMovePreviewsPrefab.GetComponent<RectTransform>().rect.width * (difference -nextMoves.Count / 2f);
                Image I = Instantiate(enemyMovePreviewsPrefab, new Vector3(), Quaternion.identity, enemyUIs[i].movePreview).GetComponent<Image>();
                I.GetComponentInChildren<TextMeshProUGUI>().text = nextMoves[j].value(current[i], player).ToString();
                I.sprite = move.typeImages[nextMoves[j].type];
                I.transform.localPosition = new Vector3(xPos, 0, 0);

                if (amount > 1) {
                    xPos = enemyMovePreviewsPrefab.GetComponent<RectTransform>().rect.width * (difference + .5f -nextMoves.Count / 2f);
                    TextMeshProUGUI t = Instantiate(enemyMovePreviewsTimesPrefab, new Vector3(), Quaternion.identity, enemyUIs[i].movePreview).GetComponent<TextMeshProUGUI>();
                    t.text = $"x{amount}";
                    t.transform.localPosition = new Vector3(xPos, 0, 0);

                    difference ++;
                }
                
                difference ++;
                amount = 0;
            }
        }
    }
}