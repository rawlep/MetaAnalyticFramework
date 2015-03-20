using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace paperTestsCode
{
    class Groups
    {


        /// <summary>
        /// An enumeration type for comparing values
        /// </summary>
        public enum Comprison { SrictlyLess, LessOrEqual, Proximity };

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"> A Type to be instantiated to the elements of the list </typeparam>
        /// <param name="function">The relationship functions to use to identifying founder sets and defining the "fitness function"</param>
        /// <param name="comp">An emumeration type specifying the way to compare values when processing the list of potentail combinations</param>
        /// <param name="lst">The input list of elements of type </param>
        /// <param name="eta">The tolerance for forming groups.  </param>
        /// <param name="tau">The least upper bound on the relationship </param>
        /// <returns> A reduced set of group models for lst</returns>
        public Tuple<List<List<List<T>>>,int> metaAnalyticProcedure<T>(Func<T, T, double> function, Comprison comp,
                                                                       List<T> lst, double eta, double tau)
        {
            // 
            Func<T, T, double> absRelation = (x, y) => (Math.Abs(function(x, y) - tau));
            Func<List<T>, double> connectedness_aux = ls => apply_to_adjecent_elms(absRelation, ls, 0);
            Func<List<T>, double> connectedness = ls => (ls == null || ls.Count < 2) ? 2 : connectedness_aux(ls);
            Func<List<List<T>>, double> modelConnectedness = ls => (ls.Select(x => connectedness(x))).Average();

  

            // ----------------------------------- The method -------------------------------------------------------
            // 1. Get the initial founder sets using the relation function and the stated tolerance
            List<List<T>> founderSets = getFounderSets(lst, function, eta);            
            
            // 2. consider ways of recombining the founder sets to form larger groups  
            var recombinedFounders_aux = allGroups(founderSets, true);
            var recombinedFounders = recombinedFounders_aux.Select(x => x.Select(y => listConcatenate(y)).ToList()).ToList();
            int original_recombinations = founderSets.Count;
            
            // 3. return the reduced set of possibilities
            var result = maintainBy(modelConnectedness, comp, recombinedFounders);
            return Tuple.Create(result, original_recombinations); 
        }


        // apply a function to  adjacent elements of the list
        private double apply_to_adjecent_elms<T>(Func<T,T,double> fun, List<T> list,double dummy)
        {
            int len = list.Count;
            var temp = new List<double>();
            if (len < 2)
            {   
                return dummy; 
            }
            else
            {
                for (int i = 0; i < len - 1; i++)
                    temp.Add(fun (list.ElementAt(i),list.ElementAt(i+1)) );
                return temp.Average();
            }
        }

        /// <summary>
        /// Retaining elements in a list that satisfy a given condition such that only those elements of incraesing / decreasing
        /// subject to some real function on its elements, remains in the list
        /// </summary>
        private List<T> maintainBy<T>(Func<T, double> fn, Comprison comp, List<T> ls)
        {
            if (ls != null && ls.Count > 1)
            {
                var y = ls.ElementAt(0);
                var lss = drop(1, ls);
                return maintainBy_aux(fn, y, lss, comp, null);
            }
            else
                return ls;
        }

        //
        private List<T> maintainBy_aux<T>(Func<T, double> fn, T a1, List<T> ls, Comprison comp, List<T> acc)
        {
            
            if (ls != null && ls.Count > 0)
            {
                acc = (acc == null) ? new List<T>() : acc;
                var y = ls.ElementAt(0);
                ls.RemoveAt(0);

                // comparing the elements depending on the comparison operator
                Func<double, double, bool> compare = (a, b) => comp == Comprison.Proximity ? Math.Abs(b - a) < 0.03 : (comp == Comprison.SrictlyLess ? a < b :  a <= b) ;

                if (compare(fn(a1), fn(y)))
                {
                    prepend_to_list(a1, acc);
                    //acc.Insert(0,a1);
                    return maintainBy_aux(fn, a1, ls, comp, acc);
                }
                else
                {
                    prepend_to_list(y, acc);
                    //acc.Insert(0, y);
                    return maintainBy_aux(fn, y, ls, comp, acc);
                }
            }
            else
                return acc;
        }

        // prepends an element to a list if a comparable element is not already at the front of said list
        private static void prepend_to_list<T>(T a, List<T> ls)
        {
            if (ls == null || ls.Count < 1)
                ls.Add(a);
            else
            {
                var x = ls.ElementAt(0);
                if (!EqualityComparer<T>.Default.Equals(x, a))
                    ls.Insert(0, a);
            }
        }

        // taking the first n elements of a list. I prefer this syntax
        private static List<T> take<T>(int n, List<T> list)
        {
            return list.Take(n).ToList();
        }


        // The analogue of take - dropping the first n elements of a list  
        private static List<T> drop<T>(int n, List<T> list)
        {
            var split = splitAt(n, list);
            if (split == null)
                return null;
            else
                return split.Item2;
        }


        /// <summary>
        /// Split a list at a given index, returning a tuple with each portion of the list, or null if the 
        /// list is null or the index is invalid.
        /// </summary>
        private static Tuple<List<T>, List<T>> splitAt<T>(int n, List<T> ls)
        {
            if (ls == null || n <= 0 || n > ls.Count)
                return null;
            else
            {
                List<T> front = new List<T>();
                List<T> back  = new List<T>();
                for (int i = 0; i < ls.Count; i++)
                {
                    if (i < n)
                        front.Add(ls.ElementAt(i));
                    else
                        back.Add(ls.ElementAt(i));
                }
                return Tuple.Create(front, back);
            }
        }

        /// <summary> 
        /// All adjacent concatenations,with the option to concatenate at least two elements to form a group
        /// </summary>
        public List<List<List<T>>> allGroups<T>(List<T> ls, bool one_element_in_a_group)
        {
            if (ls != null && ls.Count > 0)
            {
                int len = ls.Count;
                int max = 0;
                int cuts = one_element_in_a_group ? len - 1 : Math.DivRem(len, 2, out max);
                int min_group = one_element_in_a_group ? 1 : 2;
                var temp = new List<List<List<T>>>();
                // get all the pieces;
                for (int i = 0; i < cuts; i++)
                {
                    var iPieces = (nGroups(i, min_group, ls));
                    foreach (List<List<T>> tp in iPieces)
                        temp.Add(tp);
                }
                return temp;
            }
            else
                return null;

        }

        /// <summary>
        /// Calculates all possible adjacent elements from n cuts of the list built from
        /// at least min_lim elements, keeping track of the occurrence of cuts
        /// </summary>
        private List<List<List<T>>> nGroups<T>(int m, int min_lim, List<T> inList)
        {
            if (inList == null || min_lim < 1 || m >= inList.Count)
                return null;
            else if (m <= 0)
            {
                // return the original list
                var temp = new List<List<T>>();
                temp.Add(inList);
                var temp2 = new List<List<List<T>>>();
                temp2.Add(temp);
                return temp2;
            }
            else
            {
                int end = inList.Count - min_lim + 1;
                List<List<List<T>>> temp = new List<List<List<T>>>();
                for (int i = min_lim; i < end; i++)
                {
                    // the elements of the list up to i
                    var split = splitAt(i, inList);
                    List<T> ns = split.Item1;
                    List<T> ms = split.Item2;
                    var sub_groups = nGroups(m - 1, min_lim, ms);
                    sub_groups = sub_groups == null ? new List<List<List<T>>>() : sub_groups;
                    foreach (List<List<T>> ps in sub_groups)
                    {
                        var grp = (ps == null) ? new List<List<T>>() : ps;
                        grp.Insert(0, ns);
                        temp.Add(grp);
                    }
                }
                return temp;
            }
        }

        // the find groups function
        private List<List<T>> getFounderSets<T>(List<T> list, Func<T, T, double> fun, double tol)
        {
            return findGrps_aux<T>(list, fun, tol, null);
        }

        static List<List<T>> findGrps_aux<T>(List<T> list, Func<T, T, double> fun, double tol, List<List<T>> accm)
        {
            if (list == null)
                return accm;
            else if (list.Count == 1)
            {
                accm.Add(list);
                return accm;
            }
            else
            {
                var elm = list.ElementAt(0);
                var effective_set = findGrps_aux1(1, list, elm, fun, tol, null);
                int mx = effective_set == null ? 1 : effective_set.Max(x => x.Item2);
                var split = splitAt(mx, list);
                accm = accm == null ? new List<List<T>>() : accm;
                accm.Add(split.Item1);

                return findGrps_aux(split.Item2, fun, tol, accm);
            }
        }

        /// find groups
        private static List<Tuple<List<T>, int>> findGrps_aux1<T>(int n, List<T> list, T elm, Func<T, T, double> fun, double tol, List<Tuple<List<T>, int>> dummy)
        {
            if (n >= 0 &&  list != null)
            {
                if (n > list.Count - 1)
                    return dummy;
                else
                {
                    var front  = take(n, list);
                    bool valid = front.All(x => fun(elm, x) >= tol);
                    if (valid == false)
                    {
                        return dummy;
                    }
                    else
                    {
                        dummy = (dummy == null) ? new List<Tuple<List<T>, int>>() : dummy;
                        dummy.Insert(0, Tuple.Create(front, front.Count));
                        return findGrps_aux1(n + 1, list, elm, fun, tol, dummy);
                    }
                }
            }
            else
                return null;
        }

        // concatenate a list of lists
        public List<T> listConcatenate<T>(List<List<T>> ls)
        {
            if (ls != null && ls.Count > 0)
            {
                List<T> temp = new List<T>();
                for (int i = 0; i < ls.Count; i++)
                    foreach (T elm in ls.ElementAt(i))
                        temp.Add(elm);
                return temp;
            }
            else
                return null;
        }
    }

}
