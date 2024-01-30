using System;
using System.Collections.Generic;
using System.Linq;

namespace CalculateScrapForQuota.Utils;

public class MathUtil
{
    public static (List<T> combination, int totalValue) FindBestCombination<T>(
        List<T> items, 
        int quota, 
        Func<T, int> valueSelector)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));
        if (valueSelector == null) throw new ArgumentNullException(nameof(valueSelector));
        if (quota < 0) throw new ArgumentException("Quota cannot be negative.", nameof(quota));
        
        int n = items.Count;
        int maxValue = items.Sum(item => valueSelector(item));
        int[,] dp = new int[n + 1, maxValue + 1];

        for (int i = 0; i <= n; i++)
        {
            for (int j = 0; j <= maxValue; j++)
            {
                if (i == 0 || j == 0)
                    dp[i, j] = 0;
                else
                {
                    int itemValue = valueSelector(items[i - 1]);
                    if (itemValue <= j)
                        dp[i, j] = Math.Max(dp[i - 1, j], dp[i - 1, j - itemValue] + itemValue);
                    else
                        dp[i, j] = dp[i - 1, j];
                }
            }
        }

        // Find the smallest total value greater than or equal to the quota
        int totalValue = 0;
        for (int j = quota; j <= maxValue; j++)
        {
            if (dp[n, j] >= quota)
            {
                totalValue = j;
                break;
            }
        }

        // Backtrack to find the items
        var result = new List<T>();
        for (int i = n, j = totalValue; i > 0 && j > 0; i--)
        {
            if (dp[i, j] != dp[i - 1, j])
            {
                result.Add(items[i - 1]);
                j -= valueSelector(items[i - 1]);
            }
        }

        result.Reverse(); // Reverse to get the correct order
        return (result, totalValue);
    }
}

