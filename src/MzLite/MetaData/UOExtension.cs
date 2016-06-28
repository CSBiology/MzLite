#region license
// The MIT License (MIT)

// UOExtension.cs

// Copyright (c) 2016 Alexander Lüdemann
// alexander.luedemann@outlook.com
// luedeman@rhrk.uni-kl.de

// Computational Systems Biology, Technical University of Kaiserslautern, Germany


// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using MzLite.Model;

namespace MzLite.MetaData.UO
{

    public static class UO
    {


        #region volumn units

        public static TPC UO_Liter<TPC>(this IHasUnit<TPC> attr) where TPC : IParamContainer
        {
            return attr.SetUnit("UO:0000099");
        }

        #endregion

        #region concentration units

        public static TPC UO_GramPerLiter<TPC>(this IHasUnit<TPC> attr) where TPC : IParamContainer
        {
            return attr.SetUnit("UO:0000175");
        }

        #endregion

        #region mass units

        public static TPC UO_Dalton<TPC>(this IHasUnit<TPC> attr) where TPC : IParamContainer
        {
            return attr.SetUnit("UO:0000221");
        }

        #endregion

        #region energy units

        public static TPC UO_Electronvolt<TPC>(this IHasUnit<TPC> attr) where TPC : IParamContainer
        {
            return attr.SetUnit("UO:0000266");
        }

        #endregion

        #region time units

        public static TPC UO_Second<TPC>(this IHasUnit<TPC> attr) where TPC : IParamContainer
        {
            return attr.SetUnit("UO:0000010");
        }

        public static TPC UO_Minute<TPC>(this IHasUnit<TPC> attr) where TPC : IParamContainer
        {
            return attr.SetUnit("UO:0000031");
        }

        #endregion
    }
}
