using System.Collections;
using System.Collections.Generic;

namespace Talent.GraphEditor.Core
{
    /// <summary>
    /// Двухсторонний словарь
    /// </summary>
    /// <typeparam name="TKey">Тип ключа</typeparam>
    /// <typeparam name="TValue">Тип значения</typeparam>
    public class BidirectionalDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, TValue> _forward = new();
        private Dictionary<TValue, TKey> _reverse = new();

        /// <summary>
        /// Возвращает ключи в словаре
        /// </summary>
        public IReadOnlyCollection<TKey> Keys => _forward.Keys;

        /// <summary>
        /// Возвращает значения в словаре
        /// </summary>
        public IReadOnlyCollection<TValue> Values => _forward.Values;

        /// <summary>
        /// Добавляет новое значение по ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        public void Add(TKey key, TValue value)
        {
            _forward.Add(key, value);
            _reverse.Add(value, key);
        }

        /// <summary>
        /// Устанавливает новое значение по ключу (перезаписывает)
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        public void Set(TKey key, TValue value)
        {
            _forward[key] = value;
            _reverse[value] = key;
        }

        /// <summary>
        /// Получает значение по ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Искомое значение</returns>
        public TValue Get(TKey key)
        {
            return _forward[key];
        }

        /// <summary>
        /// Получает значение по ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Искомое значение</returns>
        public TKey Get(TValue key)
        {
            return _reverse[key];
        }

        /// <summary>
        /// Удаляет ключ из словаря
        /// </summary>
        /// <param name="key">Ключ</param>
        public void Remove(TKey key)
        {
            _reverse.Remove(_forward[key]);
            _forward.Remove(key);
        }

        /// <summary>
        /// Удаляет значение из словаря
        /// </summary>
        /// <param name="key">Значение</param>
        public void Remove(TValue key)
        {
            _forward.Remove(_reverse[key]);
            _reverse.Remove(key);
        }

        /// <summary>
        /// Пытается получить значение по ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Возвращает искомое значение, если ключ найден, иначе null</param>
        /// <returns>true, если ключ найден, иначе false</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _forward.TryGetValue(key, out value);
        }

        /// <summary>
        /// Пытается получить значение по ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Возвращает искомое значение, если ключ найден, иначе null</param>
        /// <returns>true, если ключ найден, иначе false</returns>
        public bool TryGetValue(TValue key, out TKey value)
        {
            return _reverse.TryGetValue(key, out value);
        }

        /// <summary>
        /// Проверяет, что словарь содержит ключ
        /// </summary>
        /// <param name="key">true, если ключ найден, иначе false</param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            return _forward.ContainsKey(key);
        }

        /// <summary>
        /// Проверяет, что словарь содержит значение по ключу
        /// </summary>
        /// <param name="value">true, если ключ найден, иначе false</param>
        /// <returns></returns>
        public bool ContainsValue(TValue value)
        {
            return _forward.ContainsValue(value);
        }

        /// <summary>
        /// Очищает словарь
        /// </summary>
        public void Clear()
        {
            _forward.Clear();
            _reverse.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Возвращает перечислитель словаря
        /// </summary>
        /// <returns>Перечислитель словаря</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _forward.GetEnumerator();
        }
    }
}