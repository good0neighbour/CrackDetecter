using System.Collections.Generic;
using System.Drawing;

namespace CrackDetector
{
    public class FloodFill
    {
        private List<Position> mp_positions = new List<Position>();
        private Bitmap mp_image = null;
        readonly private Color t_tarCol = Color.FromArgb(255, 0, 0);


        public FloodFill(Bitmap tp_image)
        {
            mp_image = tp_image;
        }


        /// <summary>
        /// FloodFill을 시작한다.
        /// </summary>
        public void Begin(ushort t_i, ushort t_j, out List<Position> tp_pos)
        {
            mp_positions.Clear();

            ProceedFloodFill(t_i, t_j);

            tp_pos = mp_positions;
        }


        /// <summary>
        /// 현재 위치를 저장하고 다음 좌표 색상을 비교한다. 재귀함수로 동작한다.
        /// </summary>
        private void ProceedFloodFill(ushort t_i, ushort t_j)
        {
            // 현재 위치 저장
            mp_positions.Add(new Position(t_i, t_j));

            for (byte t_k = 0; t_k < 3; ++t_k)
            {
                for (int t_l = 0; t_l < 3; ++t_l)
                {
                    int t_x = t_i;
                    int t_y = t_j;

                    // 다음 수평 좌표
                    switch (t_k)
                    {
                        case 0:
                            --t_x;
                        break;

                        case 2:
                            ++t_x;
                            break;

                        default:
                            break;
                    }

                    // 다음 수직 좌표
                    switch (t_l)
                    {
                        case 0:
                            --t_y;
                        break;

                        case 2:
                            ++t_y;
                            break;

                        default:
                            break;
                    }

                    // 유효한 좌표인지
                    if (t_x >= mp_image.Width || t_x < 0
                        || t_y >= mp_image.Height || t_y < 0)
                    {
                        continue;
                    }

                    // 이미 확인한 좌표인지
                    if (mp_positions.Contains(new Position((ushort)t_x, (ushort)t_y)))
                    {
                        continue;
                    }

                    // 다음 좌표 색상 비교
                    if (mp_image.GetPixel(t_x, t_y) == t_tarCol)
                    {
                        ProceedFloodFill((ushort)t_x, (ushort)t_y);
                    }
                }
            }
        }
    }
}
