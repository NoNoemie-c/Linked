using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

    public static class extensions 
    {
        public static string[] alphanumeric = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f"};

        public static string simplify(this string s) => 
            s.ToLower().Replace(".", "").Replace(",", "");

        public static string ToBase(this Color c, int b) {
            string s = "";

            s += alphanumeric[Mathf.FloorToInt(c.r * 255 / b)] + alphanumeric[Mathf.FloorToInt(c.r * 255 - Mathf.FloorToInt(c.r * 255 / b))];
            s += alphanumeric[Mathf.FloorToInt(c.g * 255 / b)] + alphanumeric[Mathf.FloorToInt(c.g * 255 - Mathf.FloorToInt(c.g * 255 / b))];
            s += alphanumeric[Mathf.FloorToInt(c.b * 255 / b)] + alphanumeric[Mathf.FloorToInt(c.b * 255 - Mathf.FloorToInt(c.b * 255 / b))];
            s += alphanumeric[Mathf.FloorToInt(c.a * 255 / b)] + alphanumeric[Mathf.FloorToInt(c.a * 255 - Mathf.FloorToInt(c.a * 255 / b))];

            return s;
        }

        public static int AmountOf(this string s, char c) {
            int amount = 0;

            foreach (char ch in s)
                if (ch == c) 
                    amount ++;

            return amount;
        }

        public static E GetElement<E, T>(this Dictionary<List<T>, E> dict, T key) {
            foreach (List<T> list in dict.Keys) 
                if (list.Contains(key))
                    return dict[list];

            return default(E);
        }
        
        public static bool ContainsRange<T>(this List<T> table, List<T> range) {
            foreach (T item in table)
                if (range.Contains(item))
                    return true;

            return false;
        }
        public static bool ContainsRange<T>(this T[] table, List<T> range) {
            foreach (T item in table)
                if (range.Contains(item))
                    return true;

            return false;
        }
        public static bool ContainsRange<T>(this List<T> table, T[] range) {
            foreach (T item in table)
                if (range.Contains(item))
                    return true;

            return false;
        }
        public static bool ContainsRange<T>(this T[] table, T[] range) {
            foreach (T item in table)
                if (range.Contains(item))
                    return true;

            return false;
        }

        public static T Rand<T>(T[] list, float[] rates) {
            float sum = rates.Sum();

            float f = Random.Range(0, sum);

            int i = 0;
            while (i < list.Length && rates[i] >= f) {
                f -= rates[i];

                i ++;
            }
            if (i > 0)
                i --;

            return list[i];
        }

        public static T pickRandom<T>(this IEnumerable<T> list) => 
            list.ElementAt(UnityEngine.Random.Range(0, list.Count()));
        public static T pickRandom<T>(this IEnumerable<T> list, int len) => 
            list.ElementAt(UnityEngine.Random.Range(0, len));

        public static TKey KeyOf<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue val) {
            foreach (var pair in dict) 
                if (EqualityComparer<TKey>.Equals(pair.Value, val)) 
                    return pair.Key;

            return default;
        }
    }