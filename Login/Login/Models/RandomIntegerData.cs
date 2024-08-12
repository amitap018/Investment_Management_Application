using System;
using System.Collections.Generic;

namespace Login.Models
{
    public static class RandomIntegerData
    {
        public static List<int> BalancedFundPerformance { get; private set; }
        public static List<int> GlobalDebtFundPerformance { get; private set; }
        public static List<int> GlobalEquityFundPerformance { get; private set; }
        public static List<int> HighCapBalancedFundPerformance { get; private set; }
        public static List<int> HighCapDebtFundPerformance { get; private set; }
        public static List<int> LowCapDebtFundPerformance { get; private set; }
        public static List<int> LowCapEquityFundPerformance { get; private set; }
        public static List<int> MidCapBalancedFundPerformance { get; private set; }
        public static List<int> MidCapDebtFundPerformance { get; private set; }
        public static List<int> MidCapEquityFundPerformance { get; private set; }

        static RandomIntegerData()
        {
            // Initialize the list of random integers
            var random = new Random();
            BalancedFundPerformance = new List<int>();

            for (int i = 0; i < 31; i++)
            {
                BalancedFundPerformance.Add(random.Next(-10, 11)); // Generate a random integer between -10 and 10
            }

            GlobalDebtFundPerformance = new List<int>();

            for (int i = 0; i < 31; i++)
            {
                GlobalDebtFundPerformance.Add(random.Next(-10, 11)); // Generate a random integer between -10 and 10
            }

            GlobalEquityFundPerformance = new List<int>();

            for (int i = 0; i < 31; i++)
            {
                GlobalEquityFundPerformance.Add(random.Next(-10, 11)); // Generate a random integer between -10 and 10
            }

            HighCapBalancedFundPerformance = new List<int>();

            for (int i = 0; i < 31; i++)
            {
                HighCapBalancedFundPerformance.Add(random.Next(-10, 11)); // Generate a random integer between -10 and 10
            }

            HighCapDebtFundPerformance = new List<int>();

            for (int i = 0; i < 31; i++)
            {
                HighCapDebtFundPerformance.Add(random.Next(-10, 11)); // Generate a random integer between -10 and 10
            }

            LowCapDebtFundPerformance = new List<int>();

            for (int i = 0; i < 31; i++)
            {
                LowCapDebtFundPerformance.Add(random.Next(-10, 11)); // Generate a random integer between -10 and 10
            }

            LowCapEquityFundPerformance = new List<int>();

            for (int i = 0; i < 31; i++)
            {
                LowCapEquityFundPerformance.Add(random.Next(-10, 11)); // Generate a random integer between -10 and 10
            }

            MidCapBalancedFundPerformance = new List<int>();

            for (int i = 0; i < 31; i++)
            {
                MidCapBalancedFundPerformance.Add(random.Next(-10, 11)); // Generate a random integer between -10 and 10
            }

            MidCapDebtFundPerformance = new List<int>();

            for (int i = 0; i < 31; i++)
            {
                MidCapDebtFundPerformance.Add(random.Next(-10, 11)); // Generate a random integer between -10 and 10
            }

            MidCapEquityFundPerformance = new List<int>();

            for (int i = 0; i < 31; i++)
            {
                MidCapEquityFundPerformance.Add(random.Next(-10, 11)); // Generate a random integer between -10 and 10
            }

        }
    }
}
