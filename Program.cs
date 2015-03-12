using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace paperTestsCode
{
    class Program
    {
        static void Main(string[] args)
        {

            /// example from the article
            List<double[]> articleData = new List<double[]>();
            articleData.Add(new double[] { 3.199, 3.241, 3.33, 3.383, 3.439, 3.518, 3.56, 3.601, 3.708, 3.705, 3.786 });
            articleData.Add(new double[] { 3.223, 3.246, 3.321, 3.406, 3.451, 3.514, 3.555, 3.639, 3.725, 3.781, 3.857 });
            articleData.Add(new double[] { 3.204, 3.236, 3.291, 3.387, 3.474, 3.53, 3.602, 3.682, 3.752, 3.834, 3.875 });
            articleData.Add(new double[] { 3.167, 3.247, 3.346, 3.444, 3.525, 3.568, 3.652, 3.747, 3.789, 3.838, 3.943 });
            articleData.Add(new double[] { 3.196, 4.279, 3.931, 2.711, 6.156, 3.605, 2.547, 3.747, 3.838, 3.912, 4.008 });
            articleData.Add(new double[] { 7.231, 5.297, 5.303, 2.409, 1.823, 1.855, 1.841, 1.869, 3.895, 1.931, 1.931 });
            articleData.Add(new double[] { 5.24, 5.302, 5.323, 5.372, 1.801, 1.809, 1.831, 4.85, 1.864, 1.857, 1.942 });
            articleData.Add(new double[] { 5.267, 5.291, 5.364, 5.418, 1.795, 1.839, 1.838, 1.862, 1.937, 1.881, 1.923 });
            articleData.Add(new double[] { 5.263, 5.263, 5.344, 5.418, 1.788, 1.79, 1.871, 1.906, 1.868, 1.911, 1.949 });
            articleData.Add(new double[] { 5.263, 5.316, 5.354, 5.42, 1.793, 1.801, 1.858, 0.787, 1.876, 1.907, 1.94 });
            articleData.Add(new double[] { 5.221, 5.323, 5.393, 5.401, 1.828, 1.816, 1.87, 1.856, 1.887, 1.904, 1.924 });
            articleData.Add(new double[] { 5.26, 5.306, 5.315, 5.4, 1.826, 1.799, 1.84, 1.888, 1.887, 1.908, 1.929 });
            articleData.Add(new double[] { 3.269, 5.32, 7.313, 5.391, 1.783, 1.826, 1.803, 1.864, 1.869, 1.895, 1.922 });
            articleData.Add(new double[] { 5.249, 5.304, 5.356, 5.397, 1.829, 1.81, 1.845, 1.849, 1.883, 1.907, 1.915 });
            articleData.Add(new double[] { 5.262, 7.133, 4.336, 5.393, 1.824, 1.786, 1.878, 1.886, 1.881, 1.896, 1.926 });
            articleData.Add(new double[] { 5.235, 5.315, 5.349, 6.388, 1.801, 1.845, 1.872, 1.854, 1.896, 1.902, 1.933 });
            articleData.Add(new double[] { 5.268, 3.128, 5.343, 5.385, 2.775, 1.053, 1.836, 1.899, 2.313, 1.896, 0.947 });
            articleData.Add(new double[] { 5.207, 5.295, 4.369, 5.403, 1.82, 1.789, 1.849, 0.897, 1.903, 1.905, 1.912 });

            Groups gp = new Groups();

            /// the Pearson correlation delegate for testing
            Func<double[], double[], double> relation = (x, y) => pearsons(x, y);

            var optimised_result = gp.metaAnalyticProcedure(relation, Groups.Comprison.LessOrEqual, articleData, 0.75, Groups.GroupMinimum.Two);
            var meta_result = optimised_result.Item1;
            var original_recombinations = optimised_result.Item2;

            // print the groups generated from the procedure for evaluations
            Console.WriteLine("              ------------- META-ANALYTIC RESULTS -------------                     \n");
            for (int i = 0; i < meta_result.Count; i++)
            {
                var trp = meta_result.ElementAt(i);
                var j = i + 1;
                Console.WriteLine("  ------- Group configuration: {0}. No. of members: {1}  ------------------ ", j.ToString(), trp.Count.ToString());
                List<string> cuts = trp.Select(x => printSequence(x.Select(y => printSequence(y))) + "\n \n  ").ToList();
                Console.WriteLine(String.Concat(cuts));
                // Console.WriteLine("There are {0} elements in this group\n", trp.Count.ToString());
            }
            Console.WriteLine(" -------------------------------------------------------- ");
            Console.WriteLine("That's it... There are {0} possibilities, instead of {1} after recombining. Awesome!", meta_result.Count.ToString(), original_recombinations.ToString());
            Console.WriteLine("Now press any key to exit.");


            // we can now apply an analytic procedure/metric to determine which group configuration is optimum.
            // Let's call our procedure mml as was the case in the article (of course we will not just return the
            // first element of the list!).
            Func<List<List<List<double[]>>>, List<List<double[]>>> mml = groups => (groups != null && groups.Count > 0) ? groups.ElementAt(0) : new List<List<double[]>>();
            // We can now apply the metric like 
            var metric_applied_to_selected_founder_sets = mml(meta_result);
            // ... and, subsequently, analyse a smaller number of possibilities amongst which is likely to be the solution

            Console.ReadKey();
        }

        /// <summary>
        /// Pretty printing elements in an Enumerable
        /// </summary>
        static string printSequence<T>(IEnumerable<T> ls)
        {
            string temp = "";
            if (ls != null)
            {
                int len = ls.Count();
                temp += "[";
                for (int i = 0; i < len; i++)
                {
                    if (i == len - 1)
                        temp += ls.ElementAt(i).ToString() + "]";
                    else
                        temp += ls.ElementAt(i).ToString() + ",";
                }
            }
            return temp;
        }

        ///<summary>
        /// Pearson correlation coefficient for testing
        /// </summary>
        static double pearsons(IEnumerable<double> xs, IEnumerable<double> ys)
        {
            int n = Math.Min(xs.Count(), ys.Count());
            if (n > 1)
            {
                var new_xs = n == xs.Count() ? xs : xs.TakeWhile((x, i) => i < n);
                var new_ys = n == ys.Count() ? ys : ys.TakeWhile((x, i) => i < n);
                var xs_mu = new_xs.Average();
                var ys_mu = new_ys.Average();
                var sx = standard_dev(new_xs);
                var sy = standard_dev(new_ys);

                var sumxy = new_xs.Select((x, i) => x * new_ys.ElementAt(i)).Sum();
                double rxy = (sumxy - n * xs_mu * ys_mu) / ((n - 1) * sx * sy);
                return rxy;
            }
            else
                throw new Exception("Insufficient data for estimating Pearson correlation coefficient");
        }



        /// <summary>
        /// Calculate the Standard deviation
        /// </summary>
        private static double standard_dev(IEnumerable<double> values)
        {
            double mu = values.Average();
            return standard_dev_aux(values, mu);
        }

        private static double standard_dev_aux(IEnumerable<double> values, double mu)
        {
            double sumOfSquaresOfDifferences = values.Select(val => Math.Pow(val - mu, 2)).Sum();
            double n1 = values.Count() - 1;
            if (n1 != 0)
                return Math.Sqrt(sumOfSquaresOfDifferences / n1);
            else
                throw new Exception("Cannot calculate standard deviation of a sequence with less than 2 elements");

        }

    }
}

