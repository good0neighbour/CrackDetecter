using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;

namespace CrackDetector
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // 결과 이미지를 저장할 클래스
            Bitmap tp_result = LoadImage();

            // 불러온 이미지가 없을 경우 프로그램 종료
            if (tp_result == null)
            {
                return;
            }

            // 균열 영역을 기억할 자료구조
            List<Position> tp_position = null;

            // 균열 감지
            tp_result = DetectCracks(tp_result, out tp_position);

            // 자잘한 그림자 제거
            tp_result = RemoveAmbient(tp_result, tp_position);

            // 결과 이미지 저장 후 열기
            tp_result.Save($"{Application.StartupPath}\\result.png");
            Process.Start($"{Application.StartupPath}\\result.png");

            // 프로그램 대기
            Console.Write("Enter를 입력하여 프로그램 종료.");
            Console.Read();

            // 결과 이미지 제거
            File.Delete($"{Application.StartupPath}\\result.png");
        }


        static private Bitmap LoadImage()
        {
            // 대화상자
            OpenFileDialog tp_dia = new OpenFileDialog();
            tp_dia.Filter = "Image Files (*.bmp;*.jpg;*.jpeg;*.png)|*.BMP;*.JPG;*.JPEG,*.PNG";

            // 이미지 파일 불러오기
            switch (tp_dia.ShowDialog())
            {
                case DialogResult.OK:
                    return (Bitmap)Bitmap.FromFile(tp_dia.FileName);

                default:
                    return null;
            }
        }


        /// <summary>
        /// 이미지 파일을 불러온 후, 균열이 보이는 영역을 표시한 결과 이미지를 반환
        /// </summary>
        static private Bitmap DetectCracks(Bitmap tp_image, out List<Position> tp_pos)
        {
            // 균열 영역을 기억할 자료구조
            tp_pos = new List<Position>();

            // 균열 색상 경계
            Color t_cutOff = SetCutOff();

            // 이미지 높이, 너비
            ushort t_x;
            ushort t_y;

            // 이미지 높이, 너비 제한
            if (tp_image.Width >= ushort.MaxValue)
            {
                t_x = ushort.MaxValue;
            }
            else
            {
                t_x = (ushort)tp_image.Width;
            }

            if (tp_image.Height >= ushort.MaxValue)
            {
                t_y = ushort.MaxValue;
            }
            else
            {
                t_y = (ushort)tp_image.Height;
            }

            // 결과물을 저장할 비트맵 이미지
            Bitmap tp_result = new Bitmap(t_x, t_y);

            // 균열 감지
            Console.Write("픽셀 순회 중.\n");
            for (ushort t_i = 0; t_i < t_x; ++t_i)
            {
                for (ushort t_j = 0; t_j < t_y; ++t_j)
                {
                    Color t_imgCol = tp_image.GetPixel(t_i, t_j);
                    if (t_imgCol.R >= t_cutOff.R
                        && t_imgCol.G >= t_cutOff.G
                        && t_imgCol.B >= t_cutOff.B)
                    {
                        tp_result.SetPixel(t_i, t_j,
                            Color.FromArgb(
                                (byte)(t_imgCol.R + (255 - t_imgCol.R) * 0.8f),
                                (byte)(t_imgCol.G + (255 - t_imgCol.G) * 0.8f),
                                (byte)(t_imgCol.B + (255 - t_imgCol.B) * 0.8f)
                            )
                        );
                    }
                    else
                    {
                        tp_result.SetPixel(t_i, t_j, Color.FromArgb(255, 0, 0));
                        tp_pos.Add(new Position(t_i, t_j));
                    }
                }
            }

            // 결과 이미지 반환
            return tp_result;
        }


        /// <summary>
        /// 사용자로부터 균열 색상 경계로 사용할 RGB 값을 입력 받는다.
        /// </summary>
        static private Color SetCutOff()
        {
            string[] tp_inputs = null;

            while (true)
            {
                Console.Write("균열 색상 경계 R,G,B >> ");
                tp_inputs = Console.ReadLine().Split(',');

                if (tp_inputs.Length == 3)
                {
                    break;
                }
                else
                {
                    Console.Write("유효하지 않은 입력.\n");
                }
            }
            
            return Color.FromArgb(
                byte.Parse(tp_inputs[0]),
                byte.Parse(tp_inputs[1]),
                byte.Parse(tp_inputs[2])
            );
        }


        static private Bitmap RemoveAmbient(Bitmap tp_image, List<Position> tp_position)
        {
            FloodFill tp_floodFill = new FloodFill(tp_image);
            bool[,] tp_check = new bool[tp_image.Width, tp_image.Height];

            // 최소 균열 면적 입력
            Console.Write("최소 균열 영역 면적(px) >> ");
            byte t_minArea = byte.Parse(Console.ReadLine());

            // 픽셀 순회
            Console.Write("픽셀 순회 중.\n");
            foreach (Position t_area in tp_position)
            {
                switch (tp_check[t_area.t_X, t_area.t_Y])
                {
                    case true:
                        // 이미 확인 한 경우
                        continue;

                    case false:
                        if (tp_image.GetPixel(t_area.t_X, t_area.t_Y) != Color.FromArgb(255, 0, 0))
                        {
                            break;
                        }

                        // 제거 대상인지 확인할 용도
                        bool t_remove = false;

                        // 영역 저장할 용도
                        List<Position> tp_filled = null;

                        // FloodFill 시작
                        tp_floodFill.Begin(t_area.t_X, t_area.t_Y, out tp_filled);

                        // 영역 크기 확인
                        if (tp_filled.Count < t_minArea)
                        {
                            t_remove = true;
                        }

                        // 영역에 포함된 좌표 순회
                        foreach (Position t_pos in tp_filled)
                        {
                            tp_check[t_pos.t_X, t_pos.t_Y] = true;
                            // 제거 대상인 경우
                            if (t_remove)
                            {
                                tp_image.SetPixel(t_pos.t_X, t_pos.t_Y, Color.Black);
                            }
                        }
                        break;
                }
            }

            return tp_image;
        }
    }
}
