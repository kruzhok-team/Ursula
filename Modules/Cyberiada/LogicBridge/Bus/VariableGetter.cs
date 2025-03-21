using System;
#if UNITY && DEBUG
using UnityEngine;
#endif

namespace Talent.Logic.Bus
{
    /// <summary>
    /// Структура для получения переменных
    /// </summary>
    /// <typeparam name="T">Тип получаемой переменной</typeparam>
    public readonly struct VariableGetter<T> : IVariableGetter
    {
        private readonly Func<T> _getter;

        /// <summary>
        /// Конструктор <see cref="VariableGetter{T}"/>
        /// </summary>
        /// <param name="getter">Функция получения переменной</param>
        /// <exception cref="ArgumentNullException">Если функция получения переменной равна null, выбрасывается исключение</exception>
        public VariableGetter(Func<T> getter)
        {
            _getter = getter ?? throw new ArgumentNullException(nameof(getter));
        }

        /// <summary>
        /// Пытается получить значение переменной по типу
        /// </summary>
        /// <param name="variable">Если найдена переменная с указанным типом, то возвращается значение, иначе null</param>
        /// <typeparam name="K">Тип переменной</typeparam>
        /// <returns>true, если переменная с указанным типом успешно получена, иначе false</returns>
        public bool TryGetTypedVariable<K>(out K variable)
        {
            if (_getter is Func<K> typedGetter)
            {
                variable = typedGetter.Invoke();
                return true;
            }

#if UNITY && DEBUG
            Debug.LogWarning($"Could not cast variable getter of type {typeof(T)} to {typeof(K)}");
#endif
            variable = default;
            return false;
        }

        /// <summary>
        /// Получает строковое представление переменной
        /// </summary>
        /// <returns>Строковое представление</returns>
        public string GetStringVariable()
        {
            T value = _getter.Invoke();

            if (value is bool variable)
            {
                return variable ? "1" : "0";
            }

            return $"{value}";
        }
    }
}
