/*************************************************************************
 *  Copyright (c) 2010 Hu Fei(xiaotie@geblab.com; geblab, www.geblab.com)
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Geb.Image
{
    using TPixel = SignedArgb64;
    using TCache = System.Int32;
    using TKernel = System.Int32;
    using TImage = Geb.Image.ImageSignedArgb64;
    using TChannel = System.Int16;

    public static partial class ImageSignedArgb64ClassHelper
    {
        

        public unsafe delegate void ActionOnPixel(TPixel* p);
        public unsafe delegate void ActionWithPosition(Int32 row, Int32 column, TPixel* p);
        public unsafe delegate Boolean PredicateOnPixel(TPixel* p);

        public unsafe static UnmanagedImage<TPixel> ForEach(this UnmanagedImage<TPixel> src, ActionOnPixel handler)
        {
            TPixel* start = (TPixel*)src.StartIntPtr;
            TPixel* end = start + src.Length;
            while (start != end)
            {
                handler(start);
                ++start;
            }
            return src;
        }

        public unsafe static UnmanagedImage<TPixel> ForEach(this UnmanagedImage<TPixel> src, ActionWithPosition handler)
        {
            Int32 width = src.Width;
            Int32 height = src.Height;

            TPixel* p = (TPixel*)src.StartIntPtr;
            for (Int32 r = 0; r < height; r++)
            {
                for (Int32 w = 0; w < width; w++)
                {
                    handler(w, r, p);
                    p++;
                }
            }
            return src;
        }

        public unsafe static UnmanagedImage<TPixel> ForEach(this UnmanagedImage<TPixel> src, TPixel* start, uint length, ActionOnPixel handler)
        {
            TPixel* end = start + src.Length;
            while (start != end)
            {
                handler(start);
                ++start;
            }
            return src;
        }

        public unsafe static Int32 Count(this UnmanagedImage<TPixel> src, PredicateOnPixel handler)
        {
            TPixel* start = (TPixel*)src.StartIntPtr;
            TPixel* end = start + src.Length;
            Int32 count = 0;
            while (start != end)
            {
                if (handler(start) == true) count++;
                ++start;
            }
            return count;
        }

        public unsafe static Int32 Count(this UnmanagedImage<TPixel> src, Predicate<TPixel> handler)
        {
            TPixel* start = (TPixel*)src.StartIntPtr;
            TPixel* end = start + src.Length;
            Int32 count = 0;
            while (start != end)
            {
                if (handler(*start) == true) count++;
                ++start;
            }
            return count;
        }

        public unsafe static List<TPixel> Where(this UnmanagedImage<TPixel> src, PredicateOnPixel handler)
        {
            List<TPixel> list = new List<TPixel>();

            TPixel* start = (TPixel*)src.StartIntPtr;
            TPixel* end = start + src.Length;
            while (start != end)
            {
                if (handler(start) == true) list.Add(*start);
                ++start;
            }

            return list;
        }

        public unsafe static List<TPixel> Where(this UnmanagedImage<TPixel> src, Predicate<TPixel> handler)
        {
            List<TPixel> list = new List<TPixel>();

            TPixel* start = (TPixel*)src.StartIntPtr;
            TPixel* end = start + src.Length;
            while (start != end)
            {
                if (handler(*start) == true) list.Add(*start);
                ++start;
            }

            return list;
        }

        /// <summary>
        /// 查找模板。模板中值代表实际像素值。负数代表任何像素。返回查找得到的像素的左上端点的位置。
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public static unsafe List<System.Drawing.Point> FindTemplate(this UnmanagedImage<TPixel> src, int[,] template)
        {
            List<System.Drawing.Point> finds = new List<System.Drawing.Point>();
            int tHeight = template.GetUpperBound(0) + 1;
            int tWidth = template.GetUpperBound(1) + 1;
            int toWidth = src.Width - tWidth + 1;
            int toHeight = src.Height - tHeight + 1;
            int stride = src.Width;
            TPixel* start = (TPixel*)src.StartIntPtr;
            for (int r = 0; r < toHeight; r++)
            {
                for (int c = 0; c < toWidth; c++)
                {
                    TPixel* srcStart = start + r * stride + c;
                    for (int rr = 0; rr < tHeight; rr++)
                    {
                        for (int cc = 0; cc < tWidth; cc++)
                        {
                            int pattern = template[rr, cc];
                            if (pattern >= 0 && srcStart[rr * stride + cc] != pattern)
                            {
                                goto Next;
                            }
                        }
                    }

                    finds.Add(new System.Drawing.Point(c, r));

                Next:
                    continue;
                }
            }

            return finds;
        }

        
    }

    public partial class ImageSignedArgb64
    {
        

        public unsafe TPixel* Start { get { return (TPixel*)this.StartIntPtr; } }

        public unsafe TPixel this[int index]
        {
            get
            {
                return Start[index];
            }
            set
            {
                Start[index] = value;
            }
        }

        public unsafe TPixel this[int row, int col]
        {
            get
            {
                return Start[row * this.Width + col];
            }
            set
            {
                Start[row * this.Width + col] = value;
            }
        }

        public unsafe TPixel this[System.Drawing.Point location]
        {
            get
            {
                return this[location.Y, location.X];
            }
            set
            {
                this[location.Y, location.X] = value;
            }
        }

        public unsafe TPixel* Row(Int32 row)
        {
            if (row < 0 || row >= this.Height) throw new ArgumentOutOfRangeException("row");
            return Start + row * this.Width;
        }

        public unsafe void Fill(TPixel pixel)
        {
            TPixel* p = this.Start;
            TPixel* end = p + this.Length;
            while (p != end)
            {
                *p = pixel;
                p++;
            }
        }

        public unsafe void Replace(TPixel pixel, TPixel replaced)
        {
            TPixel* p = this.Start;
            TPixel* end = p + this.Length;
            while (p != end)
            {
                if (*p == pixel)
                {
                    *p = replaced;
                }
                p++;
            }
        }

        public unsafe void Copy(UnmanagedImage<TPixel> src, System.Drawing.Point start, System.Drawing.Rectangle region, System.Drawing.Point destAnchor)
        {
            if (start.X >= src.Width || start.Y >= src.Height) return;
            int startSrcX = Math.Max(0, start.X);
            int startSrcY = Math.Max(0, start.Y);
            int endSrcX = Math.Min(start.X + region.Width, src.Width);
            int endSrcY = Math.Min(start.Y + region.Height, src.Height);
            int offsetX = start.X < 0? -start.X : 0;
            int offsetY = start.Y < 0? -start.Y : 0;
            offsetX = destAnchor.X + offsetX;
            offsetY = destAnchor.Y + offsetY;
            int startDstX = Math.Max(0, offsetX);
            int startDstY = Math.Max(0, offsetY);
            offsetX = offsetX < 0 ? -offsetX : 0;
            offsetY = offsetY < 0 ? -offsetY : 0;
            startSrcX += offsetX;
            startSrcY += offsetY;
            int endDstX = Math.Min(destAnchor.X + region.Width, this.Width);
            int endDstY = Math.Min(destAnchor.Y + region.Height, this.Height);
            int copyWidth = Math.Min(endSrcX - startSrcX, endDstX - startDstX);
            int copyHeight = Math.Min(endSrcY - startSrcY, endDstY - startDstY);
            if (copyWidth <= 0 || copyHeight <= 0) return;

            int srcWidth = src.Width;
            int dstWidth = this.Width;

            TPixel* srcLine = (TPixel*)(src.StartIntPtr) + srcWidth * startSrcY + startSrcX;
            TPixel* dstLine = this.Start + dstWidth * startDstY + startDstX;
            TPixel* endSrcLine = srcLine + srcWidth * copyHeight;
            while (srcLine < endSrcLine)
            {
                TPixel* pSrc = srcLine;
                TPixel* endPSrc = pSrc + copyWidth;
                TPixel* pDst = dstLine;
                while (pSrc < endPSrc)
                {
                    *pDst = *pSrc;
                    pSrc++;
                    pDst++;
                }
                srcLine += srcWidth;
                dstLine += dstWidth;
            }
        }

        public void FloodFill(System.Drawing.Point location, TPixel anchorColor, TPixel replecedColor)
        {
            int width = this.Width;
            int height = this.Height;
            if (location.X < 0 || location.X >= width || location.Y < 0 || location.Y >= height) return;

            if (anchorColor == replecedColor) return;
            if (this[location.Y, location.X] != anchorColor) return;

            Stack<System.Drawing.Point> points = new Stack<System.Drawing.Point>();
            points.Push(location);

            int ww = width - 1;
            int hh = height - 1;

            while (points.Count > 0)
            {
                System.Drawing.Point p = points.Pop();
                this[p.Y, p.X] = replecedColor;
                if (p.X > 0 && this[p.Y, p.X - 1] == anchorColor)
                {
                    this[p.Y, p.X - 1] = replecedColor;
                    points.Push(new System.Drawing.Point(p.X - 1, p.Y));
                }

                if (p.X < ww && this[p.Y, p.X + 1] == anchorColor)
                {
                    this[p.Y, p.X + 1] = replecedColor;
                    points.Push(new System.Drawing.Point(p.X + 1, p.Y));
                }

                if (p.Y > 0 && this[p.Y - 1, p.X] == anchorColor)
                {
                    this[p.Y - 1, p.X] = replecedColor;
                    points.Push(new System.Drawing.Point(p.X, p.Y - 1));
                }

                if (p.Y < hh && this[p.Y + 1, p.X] == anchorColor)
                {
                    this[p.Y + 1, p.X] = replecedColor;
                    points.Push(new System.Drawing.Point(p.X, p.Y + 1));
                }
            }
        }

        /// <summary>
        /// 使用众值滤波
        /// </summary>
        public unsafe void ApplyModeFilter(int size)
        {
            if (size <= 1) throw new ArgumentOutOfRangeException("size 必须大于1.");
            else if (size > 127) throw new ArgumentOutOfRangeException("size 最大为127.");
            else if (size % 2 == 0) throw new ArgumentException("size 应该是奇数.");

            int* vals = stackalloc int[size * size + 1];
            TPixel* keys = stackalloc TPixel[size * size + 1];

            UnmanagedImage<TPixel> mask = this.Clone() as UnmanagedImage<TPixel>;
            int height = this.Height;
            int width = this.Width;

            TPixel* pMask = (TPixel*)mask.StartIntPtr;
            TPixel* pThis = (TPixel*)this.StartIntPtr;

            int radius = size / 2;

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    int count = 0;

                    // 建立直方图
                    for (int y = -radius; y <= radius; y++)
                    {
                        for (int x = -radius; x <= radius; x++)
                        {
                            int yy = y + h;
                            int xx = x + w;
                            if (xx >= 0 && xx < width && yy >= 0 && yy < height)
                            {
                                TPixel color = pMask[yy * width + xx];

                                bool find = false;
                                for (int i = 0; i < count; i++)
                                {
                                    if (keys[i] == color)
                                    {
                                        vals[i]++;
                                        find = true;
                                        break;
                                    }
                                }

                                if (find == false)
                                {
                                    keys[count] = color;
                                    vals[count] = 1;
                                    count++;
                                }
                            }
                        }
                    }

                    if (count > 0)
                    {
                        // 求众数
                        int index = -1;
                        int max = int.MinValue;
                        for (int i = 0; i < count; i++)
                        {
                            if (vals[i] > max)
                            {
                                index = i;
                                max = vals[i];
                            }
                        }

                        if (max > 1)
                        {
                            pThis[h * width + w] = keys[index];
                        }
                    }
                }
            }

            mask.Dispose();
        }

        

        

        public unsafe TImage GaussPyramidUp()
        {
            int width = this.Width;
            int height = this.Height;
            int ww = width / 2;
            int hh = height / 2;

            TImage imgUp = new TImage(ww, hh);
            TPixel* imgStart = Start;
            TPixel* imgPyUpStart = imgUp.Start;
            int hSrc, wSrc;
            TPixel* lineSrc;
            TPixel* lineDst;
            for (int h = 0; h < hh; h++)
            {
                hSrc = 2 * h;
                lineSrc = imgStart + hSrc * width;
                lineDst = imgPyUpStart + h * ww;
                for (int w = 0; w < ww; w++)
                {
                    wSrc = 2 * w;

                    // 对于四边不够一个高斯核半径的地方，直接赋值
                    if (hSrc < 2 || hSrc > height - 3 || wSrc < 2 || wSrc > width - 3)
                    {
                        lineDst[w] = lineSrc[wSrc];
                    }
                    else
                    {
                        // 计算高斯

                        TPixel* p = lineSrc + wSrc - 2 * width;

                        TPixel p00 = p[-2];
                        TPixel p01 = p[-1];
                        TPixel p02 = p[0];
                        TPixel p03 = p[1];
                        TPixel p04 = p[2];

                        p += width;
                        TPixel p10 = p[-2];
                        TPixel p11 = p[-1];
                        TPixel p12 = p[0];
                        TPixel p13 = p[1];
                        TPixel p14 = p[2];

                        p += width;
                        TPixel p20 = p[-2];
                        TPixel p21 = p[-1];
                        TPixel p22 = p[0];
                        TPixel p23 = p[1];
                        TPixel p24 = p[2];

                        p += width;
                        TPixel p30 = p[-2];
                        TPixel p31 = p[-1];
                        TPixel p32 = p[0];
                        TPixel p33 = p[1];
                        TPixel p34 = p[2];

                        p += width;
                        TPixel p40 = p[-2];
                        TPixel p41 = p[-1];
                        TPixel p42 = p[0];
                        TPixel p43 = p[1];
                        TPixel p44 = p[2];

                        //int alpha =
                        //      1 * p00.Alpha + 04 * p01.Alpha + 06 * p02.Alpha + 04 * p03.Alpha + 1 * p04.Alpha
                        //    + 4 * p10.Alpha + 16 * p11.Alpha + 24 * p12.Alpha + 16 * p13.Alpha + 4 * p14.Alpha
                        //    + 6 * p20.Alpha + 24 * p21.Alpha + 36 * p22.Alpha + 24 * p23.Alpha + 6 * p24.Alpha
                        //    + 4 * p30.Alpha + 16 * p31.Alpha + 24 * p32.Alpha + 16 * p33.Alpha + 4 * p34.Alpha
                        //    + 1 * p40.Alpha + 04 * p41.Alpha + 06 * p42.Alpha + 04 * p43.Alpha + 1 * p44.Alpha;

                        int red =
                              1 * p00.Red + 04 * p01.Red + 06 * p02.Red + 04 * p03.Red + 1 * p04.Red
                            + 4 * p10.Red + 16 * p11.Red + 24 * p12.Red + 16 * p13.Red + 4 * p14.Red
                            + 6 * p20.Red + 24 * p21.Red + 36 * p22.Red + 24 * p23.Red + 6 * p24.Red
                            + 4 * p30.Red + 16 * p31.Red + 24 * p32.Red + 16 * p33.Red + 4 * p34.Red
                            + 1 * p40.Red + 04 * p41.Red + 06 * p42.Red + 04 * p43.Red + 1 * p44.Red;

                        int green =
                              1 * p00.Green + 04 * p01.Green + 06 * p02.Green + 04 * p03.Green + 1 * p04.Green
                            + 4 * p10.Green + 16 * p11.Green + 24 * p12.Green + 16 * p13.Green + 4 * p14.Green
                            + 6 * p20.Green + 24 * p21.Green + 36 * p22.Green + 24 * p23.Green + 6 * p24.Green
                            + 4 * p30.Green + 16 * p31.Green + 24 * p32.Green + 16 * p33.Green + 4 * p34.Green
                            + 1 * p40.Green + 04 * p41.Green + 06 * p42.Green + 04 * p43.Green + 1 * p44.Green;

                        int blue =
                              1 * p00.Blue + 04 * p01.Blue + 06 * p02.Blue + 04 * p03.Blue + 1 * p04.Blue
                            + 4 * p10.Blue + 16 * p11.Blue + 24 * p12.Blue + 16 * p13.Blue + 4 * p14.Blue
                            + 6 * p20.Blue + 24 * p21.Blue + 36 * p22.Blue + 24 * p23.Blue + 6 * p24.Blue
                            + 4 * p30.Blue + 16 * p31.Blue + 24 * p32.Blue + 16 * p33.Blue + 4 * p34.Blue
                            + 1 * p40.Blue + 04 * p41.Blue + 06 * p42.Blue + 04 * p43.Blue + 1 * p44.Blue;

                        lineDst[w] = new TPixel(red >> 8, green >> 8, blue >> 8, 255);
                    }
                }
            }
            return imgUp;
        }

        public unsafe TImage GaussPyramidDown()
        {
            int width = Width;
            int height = Height;
            int ww = width * 2;
            int hh = height * 2;

            TImage imgDown = new TImage(ww, hh);
            TPixel* imgStart = this.Start;
            TPixel* imgPyDownStart = imgDown.Start;
            int hSrc, wSrc;
            TPixel* lineSrc;
            TPixel* lineDst;

            TPixel p0, p1, p2, p3;

            // 分四种情况进行处理：
            // (1) h,w 都是偶数；
            // (2) h 是偶数， w 是奇数
            // (3) h 是奇数， w 是偶数
            // (4) h 是奇数， w 是奇数

            // h 是偶数
            for (int h = 0; h < hh; h += 2)
            {
                hSrc = h / 2;
                lineDst = imgPyDownStart + h * ww;
                lineSrc = imgStart + hSrc * width;

                // w 是偶数
                for (int w = 0; w < ww; w += 2)
                {
                    wSrc = w / 2;
                    lineDst[w] = lineSrc[wSrc];
                }

                // w 是奇数
                for (int w = 1; w < ww; w += 2)
                {
                    // 防止取到最后一列
                    wSrc = Math.Min(w / 2,width-2);

                    p0 = lineSrc[wSrc];
                    p1 = lineSrc[wSrc + 1];
                    lineDst[w] = new TPixel((TChannel)((p0.Red + p1.Red) >> 1),
                        (TChannel)((p0.Green + p1.Green) >> 1),
                        (TChannel)((p0.Blue + p1.Blue) >> 1),
                        (TChannel)((p0.Alpha + p1.Alpha) >> 1));
                }
            }

            // h 是奇数
            for (int h = 1; h < hh; h += 2)
            {
                // 防止取到最后一行
                hSrc = Math.Min(h / 2, height - 2);

                lineDst = imgPyDownStart + h * ww;
                lineSrc = imgStart + hSrc * width;

                // w 是偶数
                for (int w = 0; w < ww; w += 2)
                {
                    wSrc = w / 2;
                    p0 = lineSrc[wSrc];
                    p1 = lineSrc[wSrc + width];
                    lineDst[w] = new TPixel((TChannel)((p0.Red + p1.Red) >> 1),
                        (TChannel)((p0.Green + p1.Green) >> 1),
                        (TChannel)((p0.Blue + p1.Blue) >> 1),
                        (TChannel)((p0.Alpha + p1.Alpha) >> 1));
                }

                // w 是奇数
                for (int w = 1; w < ww; w += 2)
                {
                    // 防止取到最后一列
                    wSrc = Math.Min(w / 2, width - 2);

                    p0 = lineSrc[wSrc];
                    p1 = lineSrc[wSrc + 1];
                    p2 = lineSrc[wSrc + width];
                    p3 = lineSrc[wSrc + width + 1];
                    lineDst[w] = new TPixel((TChannel)((p0.Red + p1.Red + p2.Red + p3.Red) >> 2),
                        (TChannel)((p0.Green + p1.Green + p2.Green + p3.Green) >> 2),
                        (TChannel)((p0.Blue + p1.Blue + p2.Blue + p3.Blue) >> 2),
                        (TChannel)((p0.Alpha + p1.Alpha + p2.Alpha + p3.Alpha) >> 2));
                }
            }

            return imgDown;
        }

        public unsafe TImage FastPyramidUp4X()
        {
            int width = this.Width;
            int height = this.Height;
            int ww = width / 4;
            int hh = height / 4;

            TImage imgUp = new TImage(ww, hh);
            TPixel* imgStart = Start;
            TPixel* imgPyUpStart = imgUp.Start;
            TPixel* lineSrc;
            TPixel* lineDst;
            for (int h = 0; h < hh; h++)
            {
                lineSrc = imgStart + 4 * h * width;
                lineDst = imgPyUpStart + h * ww;
                for (int w = 0; w < ww; w++)
                {
                    lineDst[w] = lineSrc[4 * w];
                }
            }
            return imgUp;
        }

        public unsafe TImage FastPyramidUp3X()
        {
            int width = this.Width;
            int height = this.Height;
            int ww = width / 3;
            int hh = height / 3;

            TImage imgUp = new TImage(ww, hh);
            TPixel* imgStart = Start;
            TPixel* imgPyUpStart = imgUp.Start;
            TPixel* lineSrc;
            TPixel* lineDst;
            for (int h = 0; h < hh; h++)
            {
                lineSrc = imgStart +  3 * h * width;
                lineDst = imgPyUpStart + h * ww;
                for (int w = 0; w < ww; w++)
                {
                    lineDst[w] = lineSrc[3 * w];
                }
            }
            return imgUp;
        }
        public unsafe TImage FastPyramidUp2X()
        {
            int width = this.Width;
            int height = this.Height;
            int ww = width / 2;
            int hh = height / 2;

            TImage imgUp = new TImage(ww, hh);
            TPixel* imgStart = Start;
            TPixel* imgPyUpStart = imgUp.Start;
            TPixel* lineSrc;
            TPixel* lineDst;
            for (int h = 0; h < hh; h++)
            {
                lineSrc = imgStart + 2 * h * width;
                lineDst = imgPyUpStart + h * ww;
                for (int w = 0; w < ww; w++)
                {
                    lineDst[w] = lineSrc[2 * w];
                }
            }
            return imgUp;
        }


        
    }

    public partial struct SignedArgb64
    {
        

        public static Boolean operator ==(TPixel lhs, int rhs)
        {
            throw new NotImplementedException();
        }

        public static Boolean operator !=(TPixel lhs, int rhs)
        {
            throw new NotImplementedException();
        }

        public static Boolean operator ==(TPixel lhs, double rhs)
        {
            throw new NotImplementedException();
        }

        public static Boolean operator !=(TPixel lhs, double rhs)
        {
            throw new NotImplementedException();
        }

        public static Boolean operator ==(TPixel lhs, float rhs)
        {
            throw new NotImplementedException();
        }

        public static Boolean operator !=(TPixel lhs, float rhs)
        {
            throw new NotImplementedException();
        }

        public static Boolean operator ==(TPixel lhs, TPixel rhs)
        {
            return lhs.Equals(rhs);
        }
        
        public static Boolean operator !=(TPixel lhs, TPixel rhs)
        {
            return !lhs.Equals(rhs);
        }

        
    }
}

