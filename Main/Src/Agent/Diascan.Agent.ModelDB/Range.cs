using System;
using System.Collections.Generic;

namespace Diascan.Agent.ModelDB
{
    /// <summary>
    /// Тип описывающий диапазон величин
    /// </summary>    
    public struct Range<T>
    {
        public T Begin { get; set; }
        public T End { get; set; }
        public T Area { get; set; }

        public Range(T beg, T end) : this()
        {
            Begin = beg;
            End = end;
        }

        public static implicit operator Range<T>(Range<float> source)
        {
            var type = typeof(T);
            return new Range<T>((T)Convert.ChangeType(source.Begin, type),
                (T)Convert.ChangeType(source.End, type));
        }

        public override string ToString()
        {
            return $"[{Begin}:{End}]";
        }
    }

    public class NearRangeSensors
    {
        public int FirstSensorNumber { get; set; }
        public int SensorCount { get; set; }

        public NearRangeSensors() { }

        public NearRangeSensors(int firstSensorNumber, int sensorCount)
        {
            FirstSensorNumber = firstSensorNumber;
            SensorCount = sensorCount;
        }
    }


    
}
