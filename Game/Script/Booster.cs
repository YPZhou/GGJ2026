using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;

public static class Booster
{
	public static double GetBooster(double time)
    {
        double res = 1.0d;

        foreach (var checkPoint in checkPoints)
        {
            if (time > checkPoint.Time)
            {
                res = checkPoint.Boost;
                break;
            }
        }

        return res;
    }

    static List<CheckPoint> checkPoints = new List<CheckPoint>()
    {
        new CheckPoint() { Time = 20, Boost = 1.4 },
        new CheckPoint() { Time = 10, Boost = 1.2 },
    };
}

struct CheckPoint
{
    public double Time;
    public double Boost;
}