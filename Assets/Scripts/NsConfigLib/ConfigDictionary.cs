﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LitJson;

namespace NsLib.Config {

    public static class ConfigDictionary {
        public static Dictionary<K, V> ToWrap<K, V>(TextAsset asset, out bool isJson, bool isLoadAll = false) where V: ConfigBase<K> {
            isJson = false;
            if (asset == null)
                return null;
            return ToWrap<K, V>(asset.bytes, out isJson, isLoadAll);
        }

        public static Dictionary<K, V> ToWrap<K, V>(byte[] buffer, out bool isJson, bool isLoadAll = false) where V : ConfigBase<K> {
            isJson = false;
            if (buffer == null)
                return null;

            Dictionary<K, V> ret = ConfigWrap.ToObject<K, V>(buffer, isLoadAll);
            if (ret == null) {
                try {
                    string text = System.Text.Encoding.UTF8.GetString(buffer);
                    ret = JsonMapper.ToObject<Dictionary<K, V>>(text);
                    isJson = true;
                } catch {
                    ret = null;
                }
            }
            return ret;
        }

        public static void PreloadWrap<K, V>(Dictionary<K, V> maps, byte[] buffer,
            MonoBehaviour mono, out bool isJson,
            Action<IDictionary> onEnd) where V : ConfigBase<K> {

            isJson = false;
            if (maps == null || buffer == null || buffer.Length <= 0 || mono == null) {
                if (onEnd != null)
                    onEnd(null);
                return;
            }

            maps.Clear();

            MemoryStream stream = new MemoryStream(buffer);


            Coroutine cor = ConfigWrap.ToObjectAsync<K, V>(stream, maps, mono, true, onEnd);
            if (cor == null) {
                stream.Close();
                stream.Dispose();

                Dictionary<K, V> ret;
                try {
                    string text = System.Text.Encoding.UTF8.GetString(buffer);
                    maps = JsonMapper.ToObject<Dictionary<K, V>>(text);
                    ret = maps;
                    isJson = true;
                } catch {
                    ret = null;
                }

                if (onEnd != null) {
                    onEnd(ret);
                }
            }

        }

        // 预加载用
        public static void PreloadWrap<K, V>(Dictionary<K, V> maps, TextAsset asset,
            MonoBehaviour mono, out bool isJson,
            Action<IDictionary> onEnd) where V : ConfigBase<K> {
            isJson = false;
            if (maps == null || asset == null || mono == null) {
                if (onEnd != null)
                    onEnd(null);
                return;
            }

            PreloadWrap<K, V>(maps, asset.bytes, mono, out isJson, onEnd);


        }

        public static void PreloadWrap<K, V>(Dictionary<K, List<V>> maps, byte[] buffer,
            MonoBehaviour mono, out bool isJson,
            Action<IDictionary> onEnd) where V : ConfigBase<K> {

            isJson = false;
            if (maps == null || buffer == null || buffer.Length <= 0 || mono == null) {
                if (onEnd != null)
                    onEnd(null);
                return;
            }

            maps.Clear();

            MemoryStream stream = new MemoryStream(buffer);


            Coroutine cor = ConfigWrap.ToObjectListAsync<K, V>(stream, maps, mono, true, onEnd);
            if (cor == null) {
                stream.Close();
                stream.Dispose();

                Dictionary<K, List<V>> ret;
                try {
                    string text = System.Text.Encoding.UTF8.GetString(buffer);
                    maps = JsonMapper.ToObject<Dictionary<K, List<V>>>(text);
                    ret = maps;
                    isJson = true;
                } catch {
                    ret = null;
                }

                if (onEnd != null) {
                    onEnd(ret);
                }
            }

        }

        public static void PreloadWrap<K, V>(Dictionary<K, List<V>> maps, TextAsset asset,
            MonoBehaviour mono, out bool isJson,
            Action<IDictionary> onEnd) where V : ConfigBase<K> {
            isJson = false;
            if (maps == null || asset == null || mono == null) {
                if (onEnd != null)
                    onEnd(null);
                return;
            }

            PreloadWrap<K, V>(maps, asset.bytes, mono, out isJson, onEnd);
        }

        public static Dictionary<K, List<V>> ToWrapList<K, V>(TextAsset asset,
            out bool isJson,
            bool isLoadAll = false) where V : ConfigBase<K> {
            isJson = false;
            if (asset == null)
                return null;
            return ToWrapList<K, V>(asset.bytes, out isJson, isLoadAll);
        }

        public static Dictionary<K, List<V>> ToWrapList<K, V>(byte[] buffer,
            out bool isJson,
            bool isLoadAll = false) where V : ConfigBase<K> {

            isJson = false;
            if (buffer == null || buffer.Length <= 0)
                return null;

            Dictionary<K, List<V>> ret = ConfigWrap.ToObjectList<K, V>(buffer, isLoadAll);
            if (ret == null) {
                try {
                    string text = System.Text.Encoding.UTF8.GetString(buffer);
                    ret = JsonMapper.ToObject<Dictionary<K, List<V>>>(text);
                    isJson = true;
                } catch {
                    ret = null;
                }
            }
            return ret;
        }


    }

    public interface IConfigVoMap<K> {
        bool ContainsKey(K key);
        bool IsJson {
            get;
        }

        bool LoadFromTextAsset(TextAsset asset, bool isLoadAll = false);

        bool LoadFromBytes(byte[] buffer, bool isLoadAll = false);

        // 预加载
        bool Preload(TextAsset asset, UnityEngine.MonoBehaviour mono, Action<IConfigVoMap<K>> onEnd);
        bool Preload(byte[] buffer, UnityEngine.MonoBehaviour mono, Action<IConfigVoMap<K>> onEnd);
    }

    
    // 两个配置
    public class ConfigVoMap<K, V>: IConfigVoMap<K> where V: ConfigBase<K> {
        private bool m_IsJson = true;
        private Dictionary<K, V> m_Map = null;

        public struct Enumerator {

            internal Dictionary<K, V>.Enumerator Iteror {
                get;
                set;
            }

            public KeyValuePair<K, V> Current {
                get {
                    V config = Iteror.Current.Value;
                    if (config == null)
                        return new KeyValuePair<K, V>();
                    config.ReadValue();
                    return Iteror.Current;
                }
            }

            public void Dispose() {
                Iteror.Dispose();
            }
            public bool MoveNext() {
                return Iteror.MoveNext();
            }
        }

        public bool IsJson {
            get {
                return m_IsJson;
            }
        }

        public Enumerator GetEnumerator() {
            if (m_Map == null)
                return new Enumerator();
            var iter = m_Map.GetEnumerator();
            Enumerator ret = new Enumerator();
            ret.Iteror = iter;
            return ret;
        }

        public bool ContainsKey(K key) {
            if (m_Map == null)
                return false;
            return m_Map.ContainsKey(key);
        }

        public bool TryGetValue(K key, out V value) {
            value = default(V);
            if (m_Map == null)
                return false;
            if (m_IsJson) {
                return m_Map.TryGetValue(key, out value);
            }
            if (!ConfigWrap.ConfigTryGetValue<K, V>(m_Map, key, out value)) {
                value = default(V);
                return false;
            }
            return true;
        }


        public bool LoadFromTextAsset(TextAsset asset, bool isLoadAll = false) {
            if (asset == null)
                return false;
            m_Map = ConfigDictionary.ToWrap<K, V>(asset, out m_IsJson, isLoadAll);
            return m_Map != null;
        }

        public bool LoadFromBytes(byte[] buffer, bool isLoadAll = false) {
            if (buffer == null || buffer.Length <= 0)
                return false;
            m_Map = ConfigDictionary.ToWrap<K, V>(buffer, out m_IsJson, isLoadAll);
            return m_Map != null;
        }

        public V this[K key] {
            get {
                if (m_Map == null)
                    return default(V);
                V ret;
                if (m_IsJson) {
                    if (!m_Map.TryGetValue(key, out ret))
                        ret = default(V);
                    return ret;
                }
                if (!TryGetValue(key, out ret))
                    ret = default(V);
                return ret;
            }
        } 

        public bool Preload(TextAsset asset, UnityEngine.MonoBehaviour mono, Action<IConfigVoMap<K>> onEnd) {
            if (asset == null || mono == null)
                return false;
            if (m_Map == null)
                m_Map = new Dictionary<K, V>();
            else
                m_Map.Clear();
            ConfigDictionary.PreloadWrap<K, V>(m_Map, asset, mono, out m_IsJson, 
                (IDictionary maps) => {
                    IConfigVoMap<K> ret = maps != null ? this : null;
                    if (onEnd != null)
                        onEnd(ret);
                });
            return true;
        }

        public bool Preload(byte[] buffer, UnityEngine.MonoBehaviour mono, Action<IConfigVoMap<K>> onEnd) {
            if (buffer == null || mono == null)
                return false;
            if (m_Map == null)
                m_Map = new Dictionary<K, V>();
            else
                m_Map.Clear();
            ConfigDictionary.PreloadWrap<K, V>(m_Map, buffer, mono, out m_IsJson,
                (IDictionary maps) => {
                    IConfigVoMap<K> ret = maps != null ? this : null;
                    if (onEnd != null)
                        onEnd(ret);
                });
            return true;
        }
    }

    public class ConfigVoListMap<K, V> : IConfigVoMap<K> where V : ConfigBase<K> {

        private bool m_IsJson = true;
        private Dictionary<K, List<V>> m_Map = null;

        public struct Enumerator {

            internal Dictionary<K, List<V>>.Enumerator Iteror {
                get;
                set;
            }

            public KeyValuePair<K, List<V>> Current {
                get {
                    List<V> vs = Iteror.Current.Value as List<V>;
                    if (vs == null)
                        return new KeyValuePair<K, List<V>>();
                    for (int i = 0; i < vs.Count; ++i) {
                        var v = vs[i];
                        v.ReadValue();
                    }
                    return Iteror.Current;
                }
            }

            public void Dispose() {
                Iteror.Dispose();
            }
            public bool MoveNext() {
                return Iteror.MoveNext();
            }
        }

        public Enumerator GetEnumerator() {
            if (m_Map == null)
                return new Enumerator();
            Enumerator ret = new Enumerator();
            ret.Iteror = m_Map.GetEnumerator();
            return ret;
        }

        public bool IsJson {
            get {
                return m_IsJson;
            }
        }

        public bool ContainsKey(K key) {
            if (m_Map == null)
                return false;
            return m_Map.ContainsKey(key);
        }

        public bool TryGetValue(K key, out List<V> value) {
            value = null;
            if (m_Map == null)
                return false;
            if (m_IsJson) {
                return m_Map.TryGetValue(key, out value);
            }
            if (!ConfigWrap.ConfigTryGetValue<K, V>(m_Map, key, out value)) {
                value = null;
                return false;
            }
            return true;
        }

        public List<V> this[K key] {
            get {
                if (m_Map == null)
                    return null;

                List<V> ret;
                if (m_IsJson) {
                    if (!m_Map.TryGetValue(key, out ret))
                        ret = null;
                    return ret;
                }
                if (!this.TryGetValue(key, out ret))
                    ret = null;
                return ret;
            }
        }

        public bool LoadFromTextAsset(TextAsset asset, bool isLoadAll = false) {
            if (asset == null)
                return false;
            m_Map = ConfigDictionary.ToWrapList<K, V>(asset, out m_IsJson, isLoadAll);
            return m_Map != null;
        }

        public bool LoadFromBytes(byte[] buffer, bool isLoadAll = false) {
            if (buffer == null || buffer.Length <= 0)
                return false;
            m_Map = ConfigDictionary.ToWrapList<K, V>(buffer, out m_IsJson, isLoadAll);
            return m_Map != null;
        }

        public bool Preload(byte[] buffer, UnityEngine.MonoBehaviour mono, Action<IConfigVoMap<K>> onEnd) {
            if (buffer == null || mono == null || buffer.Length <= 0)
                return false;
            if (m_Map == null)
                m_Map = new Dictionary<K, List<V>>();
            else
                m_Map.Clear();
            ConfigDictionary.PreloadWrap<K, V>(m_Map, buffer, mono, out m_IsJson,
                (IDictionary maps) => {
                    IConfigVoMap<K> ret = maps != null ? this : null;
                    if (onEnd != null)
                        onEnd(ret);
                });
            return true;
        }

        // 预加载
        public bool Preload(TextAsset asset, UnityEngine.MonoBehaviour mono, Action<IConfigVoMap<K>> onEnd) {
            if (asset == null || mono == null)
                return false;
            if (m_Map == null)
                m_Map = new Dictionary<K, List<V>>();
            else
                m_Map.Clear();
            ConfigDictionary.PreloadWrap<K, V>(m_Map, asset, mono, out m_IsJson,
                (IDictionary maps) => {
                    IConfigVoMap<K> ret = maps != null ? this : null;
                    if (onEnd != null)
                        onEnd(ret);
                });
            return true;
        }
    }

}