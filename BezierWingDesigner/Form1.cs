using Plot3D;
using System;
using System.Windows.Forms;
using static Plot3D.Editor3D;

using HtmlAgilityPack;
//using System.Collections.Generic;
//using System.Net.Http;
using System.Threading.Tasks;
using BezierAirfoilDesigner;
using ScottPlot;
using System.Text.Json.Nodes;

using System.Text.Json;
using System.IO;
using System.Globalization;

namespace BezierWingDesigner
{
    public partial class Form1 : Form
    {
        readonly double minZoomRange = 0.01;
        readonly double maxZoomRange = 2500.0;

        static int number_of_points_per_planform_section = 25;
        static int number_of_points_per_airfoil_half = 25;

        static double k = 0.551784777779014;
        static double span = 2000.0;
        static double root_chord = 250.0;
        static double tip_chord = root_chord * 0.05;
        static double hinge_position_fraction = 0.75;
        static double sweep = 0.0;
        static double trailing_edge_thickness = 0.5;

        static double hinge_position = root_chord * hinge_position_fraction;
        static double tip_LE_position = -hinge_position + tip_chord * hinge_position_fraction;
        static double tip_TE_position = -hinge_position - tip_chord * hinge_position_fraction;
        static double LE_tip_control_point_position = tip_LE_position * (1-k);
        static double TE_tip_control_point_position = tip_TE_position - (Math.Abs(root_chord) - Math.Abs(tip_TE_position)) * k;
        
        List<List<PointD>> planform_control_points_LE = new()
        {
            new()
            {
                new PointD(span / 2, tip_LE_position - sweep, 0.0),
                new PointD(span / 2, LE_tip_control_point_position - sweep, 0.0),
                new PointD(span / 2 * k, 0.0 - sweep * k, 0.0),
                new PointD(0.0, 0.0, 0.0)
            },

            new()
            {
                new PointD(0.0, 0.0, 0.0),
                new PointD(-span / 2 * k, 0.0 - sweep * k, 0.0),
                new PointD(-span / 2, LE_tip_control_point_position - sweep, 0.0),
                new PointD(-span / 2, tip_LE_position - sweep, 0.0)
            }
        };

        List<List<PointD>> planform_control_points_TE = new()
        {
            new()
            {
                new PointD(span / 2, tip_TE_position - sweep, 0.0),
                new PointD(span / 2, TE_tip_control_point_position - sweep, 0.0),
                new PointD(span / 2 * k,-root_chord - sweep * k, 0.0),
                new PointD(0.0,-root_chord, 0.0)
            },

            new()
            {
                new PointD(0.0,-root_chord, 0.0),
                new PointD(-span / 2 * k,-root_chord - sweep * k, 0.0),
                new PointD(-span / 2, TE_tip_control_point_position - sweep, 0.0),
                new PointD(-span / 2, tip_TE_position - sweep, 0.0)
            }
        };

        List<List<PointD>> planform_curve_points_LE = new();
        List<List<PointD>> planform_curve_points_TE = new();

        List<List<PointD>> airfoil_control_points = new()
        {
            new()
            {
                new PointD(0.0000000000000000, 0.0000000000000000, 0.0),
                new PointD(0.0000000000000000, 0.0144573876738672, 0.0),
                new PointD(0.0251460394591507, 0.0480767745161672, 0.0),
                new PointD(0.1721424698113208, 0.0824041004345160, 0.0),
                new PointD(0.2629447173668014, 0.0364800606824565, 0.0),
                new PointD(0.3454587070825096, 0.0770102150648193, 0.0),
                new PointD(0.5009103749509788, 0.0308643354578807, 0.0),
                new PointD(0.6500196391789768, 0.0472797622262542, 0.0),
                new PointD(0.8395576066138191, -0.0023389811838496, 0.0),
                new PointD(1.0000000000000000, 0.0000000000000000, 0.0)
            },

            new()
            {
                new PointD(0.0000000000000000, 0.0000000000000000, 0.0),
                new PointD(0.0000000000000000, -0.0156856296667000, 0.0),
                new PointD(0.0736697495772509, -0.0232710679838285, 0.0),
                new PointD(0.0911912282645849, -0.0184253299121407, 0.0),
                new PointD(0.2081271911545573, -0.0438452783245577, 0.0),
                new PointD(0.3566147730075391, -0.0083193958204370, 0.0),
                new PointD(0.5371624584890762, -0.0333758439323835, 0.0),
                new PointD(0.6688111284887106, -0.0056767542357767, 0.0),
                new PointD(0.8150091377358929, -0.0119037158553079, 0.0),
                new PointD(1.0000000000000000, 0.0000000000000000, 0.0)
            }
        };

        List<List<PointD>> airfoil_curve_points = new();

        List<List<PointD>> surface_points = new();
        List<List<PointD>> stl_surface_points = new();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            calculations();
            DemoSurface(ePolygonMode.Fill);
        }

        private void Editor3d_Load(object sender, EventArgs e)
        {
            base.OnLoad(e);
            Editor3d.Clear();
            Editor3d.Recalculate(false);
            Editor3d.MouseControl = eMouseCtrl.L_Theta_L_Phi;
            Editor3d.Raster = eRaster.Off;
            Editor3d.BackColor = SystemColors.Control /*Color.FromArgb(50, 50, 50)*/;
            Editor3d.TopLegendColor = Color.Empty;
        }

        private void formsPlot1_Load(object sender, EventArgs e)
        {
            formsPlot1.Plot.AxisScaleLock(enable: true, scaleMode: ScottPlot.EqualScaleMode.PreserveX);
            formsPlot1.Configuration.RightClickDragZoom = false;

            formsPlot1.Plot.XAxis.SetZoomInLimit(minimumSpan: minZoomRange);
            formsPlot1.Plot.XAxis.SetZoomOutLimit(maximumSpan: maxZoomRange);
            //formsPlot1.Plot.XAxis.TickLabelNotation(invertSign: true);

            formsPlot1.Plot.YAxis.SetZoomInLimit(minimumSpan: minZoomRange);
            formsPlot1.Plot.YAxis.SetZoomOutLimit(maximumSpan: maxZoomRange);
            //formsPlot1.Plot.YAxis.TickLabelNotation(invertSign: true);
        }

        private void calculations()
        {
            //---------------------------------------------------------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------------------------------------------------------
            // 

            for (int i = 0; i < planform_control_points_LE.Count; i++)
            {

                planform_curve_points_LE.Add(new List<PointD>());
                planform_curve_points_LE[i] = DeCasteljau.BezierCurve(planform_control_points_LE[i], number_of_points_per_planform_section);

                var plottedPlanformCurvePoints_LE = formsPlot1.Plot.AddScatterList(color: Color.Red, lineStyle: ScottPlot.LineStyle.Solid);

                for (int k = 0; k < planform_curve_points_LE[i].Count; k++)
                {
                    plottedPlanformCurvePoints_LE.Add(planform_curve_points_LE[i][k].X, planform_curve_points_LE[i][k].Y);
                }
                
                //-----------------------------------------------------------------------------------------------------------------------------------

                double[] xLE = new double[planform_control_points_LE[i].Count];
                double[] yLE = new double[planform_control_points_LE[i].Count];

                for (int j = 0; j < planform_control_points_LE[i].Count; j++)
                {
                    xLE[j] = planform_control_points_LE[i][j].X;
                    yLE[j] = planform_control_points_LE[i][j].Y;
                }

                var controlLE = new ScottPlot.Plottable.ScatterPlotListDraggable();
                controlLE.AddRange(xLE, yLE);
                controlLE.LineStyle = LineStyle.Dash;
                controlLE.Color = Color.Gray;
                controlLE.MarkerSize = 5;
                formsPlot1.Plot.Add(controlLE);

            }

            //---------------------------------------------------------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------------------------------------------------------

            for (int i = 0; i < planform_control_points_TE.Count; i++)
            {
                planform_curve_points_TE.Add(new List<PointD>());
                planform_curve_points_TE[i] = DeCasteljau.BezierCurve(planform_control_points_TE[i], number_of_points_per_planform_section);

                var plottedPlanformCurvePoints_TE = formsPlot1.Plot.AddScatterList(color: Color.Red, lineStyle: ScottPlot.LineStyle.Solid);

                for (int k = 0; k < planform_curve_points_TE[i].Count; k++)
                {
                    plottedPlanformCurvePoints_TE.Add(planform_curve_points_TE[i][k].X, planform_curve_points_TE[i][k].Y);
                }

                //-----------------------------------------------------------------------------------------------------------------------------------

                double[] xTE = new double[planform_control_points_TE[i].Count];
                double[] yTE = new double[planform_control_points_TE[i].Count];

                for (int j = 0; j < planform_control_points_TE[i].Count; j++)
                {
                    xTE[j] = planform_control_points_TE[i][j].X;
                    yTE[j] = planform_control_points_TE[i][j].Y;
                }

                var controlTE = new ScottPlot.Plottable.ScatterPlotListDraggable();
                controlTE.AddRange(xTE, yTE);
                controlTE.LineStyle = LineStyle.Dash;
                controlTE.Color = Color.Gray;
                controlTE.MarkerSize = 5;
                formsPlot1.Plot.Add(controlTE);

            }

            //---------------------------------------------------------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------------------------------------------------------

            for (int i = 0; i < airfoil_control_points.Count; i++)
            {
                airfoil_curve_points.Add(new List<PointD>());
                airfoil_curve_points[i] = DeCasteljau.BezierCurve(airfoil_control_points[i], number_of_points_per_airfoil_half);

                var plotted_airfoil_curve_points = formsPlot1.Plot.AddScatterList(color: Color.Red, lineStyle: ScottPlot.LineStyle.Solid);

                for (int k = 0; k < airfoil_curve_points[i].Count; k++)
                {
                    plotted_airfoil_curve_points.Add(airfoil_curve_points[i][k].X, airfoil_curve_points[i][k].Y + 0.25);
                }

                //-----------------------------------------------------------------------------------------------------------------------------------

                double[] xTE = new double[airfoil_control_points[i].Count];
                double[] yTE = new double[airfoil_control_points[i].Count];

                for (int j = 0; j < airfoil_control_points[i].Count; j++)
                {
                    xTE[j] = airfoil_control_points[i][j].X;
                    yTE[j] = airfoil_control_points[i][j].Y + 0.25;
                }

                var control_airfoil = new ScottPlot.Plottable.ScatterPlotListDraggable();
                control_airfoil.AddRange(xTE, yTE);
                control_airfoil.LineStyle = LineStyle.Dash;
                control_airfoil.Color = Color.Gray;
                control_airfoil.MarkerSize = 5;
                formsPlot1.Plot.Add(control_airfoil);

            }

            //---------------------------------------------------------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------------------------------------------------------

            surface_points.Add(new List<PointD>()); // List for top surface points
            surface_points.Add(new List<PointD>()); // List for bottom surface points
            
            stl_surface_points.Add(new List<PointD>()); // List for top surface points
            stl_surface_points.Add(new List<PointD>()); // List for bottom surface points

            // Loop over the planform sections
            for (int planform_section_index = 0; planform_section_index < planform_curve_points_LE.Count; planform_section_index++)
            {
                List<PointD> planform_section = planform_curve_points_LE[planform_section_index];

                for (int planform_point_index = 0; planform_point_index < planform_section.Count; planform_point_index++)
                {
                    PointD planform_point = planform_section[planform_point_index];
                    PointD tePoint = planform_curve_points_TE[planform_section_index][planform_point_index];

                    double local_chord = Math.Abs(planform_point.Y) - Math.Abs(tePoint.Y);

                    // Iterate over the points in the first list (top surface)
                    for (int i = 0; i < airfoil_curve_points[0].Count; i++)
                    {
                        PointD airfoil_point = airfoil_curve_points[0][i];

                        double x = planform_point.X;
                        double y = airfoil_point.X * local_chord + planform_point.Y;
                        double z = -airfoil_point.Y * local_chord + trailing_edge_thickness / 2 * airfoil_point.X;

                        // Add the surface point to the top surface list within surface_points
                        surface_points[0].Add(new PointD(x, y, z));
                    }


                    // Iterate over the points in the second list (bottom surface)
                    for (int i = 0; i < airfoil_curve_points[1].Count; i++)
                    {
                        PointD airfoil_point = airfoil_curve_points[1][i];

                        double x = planform_point.X;
                        double y = airfoil_point.X * local_chord + planform_point.Y;
                        double z = -airfoil_point.Y * local_chord - trailing_edge_thickness / 2 * airfoil_point.X;

                        // Add the surface point to the top surface list within surface_points
                        surface_points[1].Add(new PointD(x, y, z));
                    }
                }
            }


            //---------------------------------------------------------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------------------------------------------------------

            formsPlot1.Plot.AxisAuto();
            formsPlot1.Refresh();

            // Call the function to write the surface points as an STL file
            WriteSTLFile(surface_points, "wing_surface.stl");

            // Create a JSON array to store surface points
            JsonArray surfacePoints = new JsonArray();

            // Iterate over the surface points lists
            foreach (List<PointD> surfaceList in surface_points)
            {
                foreach (PointD point in surfaceList)
                {
                    // Create a JSON object for each surface point and add it to the JSON array
                    JsonObject pointObject = new JsonObject();
                    pointObject["x"] = point.X.ToString(CultureInfo.InvariantCulture);
                    pointObject["y"] = point.Y.ToString(CultureInfo.InvariantCulture);
                    pointObject["z"] = point.Z.ToString(CultureInfo.InvariantCulture);
                    surfacePoints.Add(pointObject);
                }
            }

            // Specify the file path where you want to save the JSON array
            string filePath = "surfacePoints.json";

            // Write the JSON array to the specified file path
            File.WriteAllText(filePath, surfacePoints.ToString());
        }

        // Function to write a single STL triangle to a stream
        void WriteSTLTriangle(StreamWriter writer, PointD p1, PointD p2, PointD p3, bool invert_normal)
        {
            // If invertNormal is true, invert the point order
            if (invert_normal)
            {
                PointD temp = p1;
                p1 = p3;
                p3 = temp;
            }

            // Calculate the vectors of the triangle
            double[] u = { p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z };
            double[] v = { p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z };

            // Calculate the normal vector
            double[] normal = {
            u[1] * v[2] - u[2] * v[1],
            u[2] * v[0] - u[0] * v[2],
            u[0] * v[1] - u[1] * v[0]
            };

            // Normalize the normal vector
            double length = Math.Sqrt(normal[0] * normal[0] + normal[1] * normal[1] + normal[2] * normal[2]);
            normal[0] /= length;
            normal[1] /= length;
            normal[2] /= length;

            // Write the facet and normal
            writer.WriteLine($"facet normal {normal[0].ToString(CultureInfo.InvariantCulture)} {normal[1].ToString(CultureInfo.InvariantCulture)} {normal[2].ToString(CultureInfo.InvariantCulture)}");
            writer.WriteLine("  outer loop");

            // Write the vertices of the triangle
            writer.WriteLine($"    vertex {p1.X.ToString(CultureInfo.InvariantCulture)} {p1.Y.ToString(CultureInfo.InvariantCulture)} {p1.Z.ToString(CultureInfo.InvariantCulture)}");
            writer.WriteLine($"    vertex {p2.X.ToString(CultureInfo.InvariantCulture)} {p2.Y.ToString(CultureInfo.InvariantCulture)} {p2.Z.ToString(CultureInfo.InvariantCulture)}");
            writer.WriteLine($"    vertex {p3.X.ToString(CultureInfo.InvariantCulture)} {p3.Y.ToString(CultureInfo.InvariantCulture)} {p3.Z.ToString(CultureInfo.InvariantCulture)}");

            writer.WriteLine("  endloop");
            writer.WriteLine("endfacet");
        }


        void WriteSTLFile(List<List<PointD>> surfacePoints, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write the header of the STL file
                writer.WriteLine("solid surface");

                // Iterate over each surface (assuming it's composed of triangles)
                for (int point_list_index = 0; point_list_index < surfacePoints.Count; point_list_index++)
                {
                    List<PointD> point_list = surfacePoints[point_list_index];
                    int numRows = (point_list.Count - 1) / number_of_points_per_airfoil_half; // Calculate the number of rows
                    int numCols = number_of_points_per_airfoil_half; // Number of columns

                    // Write triangles row by row
                    for (int row = 0; row < numRows; row++)
                    {
                        for (int col = 0; col < numCols - 1; col++)
                        {
                            // Calculate the indices of the points for the current triangle
                            int idx1 = row * number_of_points_per_airfoil_half + col;
                            int idx2 = row * number_of_points_per_airfoil_half + col + 1;
                            int idx3 = (row + 1) * number_of_points_per_airfoil_half + col;

                            // Write each triangle of the surface as an STL facet
                            if (point_list_index == 0) WriteSTLTriangle(writer, point_list[idx1], point_list[idx2], point_list[idx3], true);
                            else WriteSTLTriangle(writer, point_list[idx1], point_list[idx2], point_list[idx3], false);

                            // Write the next triangle
                            idx1 = row * number_of_points_per_airfoil_half + col + 1;
                            idx2 = (row + 1) * number_of_points_per_airfoil_half + col + 1;
                            idx3 = (row + 1) * number_of_points_per_airfoil_half + col;

                            if (point_list_index == 0) WriteSTLTriangle(writer, point_list[idx1], point_list[idx2], point_list[idx3], true);
                            else WriteSTLTriangle(writer, point_list[idx1], point_list[idx2], point_list[idx3], false);
                        }
                    }
                }

                // Write the end of the STL file
                writer.WriteLine("endsolid surface");
            }
        }


        private void DemoSurface(ePolygonMode e_Mode)
        {
            #region int s32_Values definition

            double[,] s32_Values = new double[,]
            {
                {1.5368, 1.5368, 1.5368, 1.5368, 1.5368, 1.5172, 1.4868, 1.4624, 1.4121, 1.3861, 1.3411, 1.2681, 1.1838, 1.1141, 1.0617, 0.9634, 0.9059, 0.9059, 0.9634, 1.0617, 1.1141, 1.1838, 1.2681, 1.3411, 1.3861, 1.4121, 1.4624, 1.4868, 1.5172, 1.5368, 1.5368, 1.5368, 1.5368, 1.5368},
                {1.5794, 1.5794, 1.5794, 1.5794, 1.5794, 1.5434, 1.4939, 1.4811, 1.4549, 1.4320, 1.4029, 1.3337, 1.2546, 1.1796, 1.1141, 1.0387, 0.9684, 0.9684, 1.0387, 1.1141, 1.1796, 1.2546, 1.3337, 1.4029, 1.4320, 1.4549, 1.4811, 1.4939, 1.5434, 1.5794, 1.5794, 1.5794, 1.5794, 1.5794},
                {1.6351, 1.6351, 1.6351, 1.6351, 1.6351, 1.5991, 1.5794, 1.5630, 1.5434, 1.5368, 1.5172, 1.4746, 1.3861, 1.3009, 1.2255, 1.1370, 1.0486, 1.0486, 1.1370, 1.2255, 1.3009, 1.3861, 1.4746, 1.5172, 1.5368, 1.5434, 1.5630, 1.5794, 1.5991, 1.6351, 1.6351, 1.6351, 1.6351, 1.6351},
                {1.7531, 1.7531, 1.7531, 1.7531, 1.7531, 1.7302, 1.7105, 1.6976, 1.6690, 1.6482, 1.6187, 1.5925, 1.5008, 1.4287, 1.3533, 1.2354, 1.1469, 1.1469, 1.2354, 1.3533, 1.4287, 1.5008, 1.5925, 1.6187, 1.6482, 1.6690, 1.6976, 1.7105, 1.7302, 1.7531, 1.7531, 1.7531, 1.7531, 1.7531},
                {1.9104, 1.9104, 1.9104, 1.9104, 1.9104, 1.8776, 1.8428, 1.8285, 1.8153, 1.7924, 1.7596, 1.7105, 1.6253, 1.5499, 1.4615, 1.3435, 1.2452, 1.2452, 1.3435, 1.4615, 1.5499, 1.6253, 1.7105, 1.7596, 1.7924, 1.8153, 1.8285, 1.8428, 1.8776, 1.9104, 1.9104, 1.9104, 1.9104, 1.9104},
                {2.0152, 2.0152, 2.0152, 2.0152, 2.0152, 1.9594, 1.9792, 1.9661, 1.9399, 1.9169, 1.8907, 1.8514, 1.7564, 1.6679, 1.5729, 1.4516, 1.3337, 1.3337, 1.4516, 1.5729, 1.6679, 1.7564, 1.8514, 1.8907, 1.9169, 1.9399, 1.9661, 1.9792, 1.9594, 2.0152, 2.0152, 2.0152, 2.0152, 2.0152},
                {2.1332, 2.1332, 2.1332, 2.1332, 2.1332, 2.0972, 2.0972, 2.0840, 2.0546, 2.0349, 2.0050, 1.9595, 1.8842, 1.7990, 1.7039, 1.5860, 1.4746, 1.4746, 1.5860, 1.7039, 1.7990, 1.8842, 1.9595, 2.0050, 2.0349, 2.0546, 2.0840, 2.0972, 2.0972, 2.1332, 2.1332, 2.1332, 2.1332, 2.1332},
                {2.2217, 2.2217, 2.2217, 2.2217, 2.2217, 2.1955, 2.1889, 2.1791, 2.1660, 2.1332, 2.1016, 2.0677, 2.0251, 1.9399, 1.8350, 1.7236, 1.6155, 1.6155, 1.7236, 1.8350, 1.9399, 2.0251, 2.0677, 2.1016, 2.1332, 2.1660, 2.1791, 2.1889, 2.1955, 2.2217, 2.2217, 2.2217, 2.2217, 2.2217},
                {2.3069, 2.3069, 2.3069, 2.3069, 2.3069, 2.2708, 2.2643, 2.2446, 2.2643, 2.2446, 2.2288, 2.1848, 2.1463, 2.0775, 1.9497, 1.8579, 1.7334, 1.7334, 1.8579, 1.9497, 2.0775, 2.1463, 2.1848, 2.2288, 2.2446, 2.2643, 2.2446, 2.2643, 2.2708, 2.3069, 2.3069, 2.3069, 2.3069, 2.3069},
                {2.3822, 2.3822, 2.3822, 2.3822, 2.3822, 2.3822, 2.3691, 2.3411, 2.3757, 2.3366, 2.3486, 2.3167, 2.2805, 2.2086, 2.0808, 1.9792, 1.8743, 1.8743, 1.9792, 2.0808, 2.2086, 2.2805, 2.3167, 2.3486, 2.3366, 2.3757, 2.3411, 2.3691, 2.3822, 2.3822, 2.3822, 2.3822, 2.3822, 2.3822},
                {2.4510, 2.4510, 2.4510, 2.4510, 2.4510, 2.4576, 2.4062, 2.4299, 2.4773, 2.4644, 2.4538, 2.4785, 2.4441, 2.3593, 2.2544, 2.1445, 2.0677, 2.0677, 2.1445, 2.2544, 2.3593, 2.4441, 2.4785, 2.4538, 2.4644, 2.4773, 2.4299, 2.4062, 2.4576, 2.4510, 2.4510, 2.4510, 2.4510, 2.4510},
                {2.5374, 2.5374, 2.5374, 2.5374, 2.5374, 2.5068, 2.4904, 2.5013, 2.5740, 2.5664, 2.5701, 2.6199, 2.6212, 2.5330, 2.4478, 2.3888, 2.3475, 2.3475, 2.3888, 2.4478, 2.5330, 2.6212, 2.6199, 2.5701, 2.5664, 2.5740, 2.5013, 2.4904, 2.5068, 2.5374, 2.5374, 2.5374, 2.5374, 2.5374},
                {2.6122, 2.6122, 2.6122, 2.6122, 2.6122, 2.5690, 2.5719, 2.5887, 2.6444, 2.6707, 2.6903, 2.7402, 2.7682, 2.7334, 2.6701, 2.7518, 2.7525, 2.7525, 2.7518, 2.6701, 2.7334, 2.7682, 2.7402, 2.6903, 2.6707, 2.6444, 2.5887, 2.5719, 2.5690, 2.6122, 2.6122, 2.6122, 2.6122, 2.6122},
                {2.5246, 2.5246, 2.5246, 2.6575, 2.6575, 2.6182, 2.6417, 2.6691, 2.7263, 2.7726, 2.8075, 2.8705, 2.9392, 2.9231, 2.9917, 3.0423, 3.0769, 3.0769, 3.0423, 2.9917, 2.9231, 2.9392, 2.8705, 2.8075, 2.7726, 2.7263, 2.6691, 2.6417, 2.6182, 2.6575, 2.6575, 2.5246, 2.5246, 2.5246},
                {2.5049, 2.5049, 2.5049, 2.6367, 2.6935, 2.6706, 2.7263, 2.7623, 2.8377, 2.9065, 2.9852, 3.0540, 3.1246, 3.1469, 3.1916, 3.2178, 3.2672, 3.2672, 3.2178, 3.1916, 3.1469, 3.1246, 3.0540, 2.9852, 2.9065, 2.8377, 2.7623, 2.7263, 2.6706, 2.6935, 2.6367, 2.5049, 2.5049, 2.5049},
                {2.5049, 2.5049, 2.5049, 2.6367, 2.7301, 2.7132, 2.7984, 2.8358, 2.9295, 3.0179, 3.0933, 3.1851, 3.2452, 3.2615, 3.2702, 3.3227, 3.3522, 3.3522, 3.3227, 3.2702, 3.2615, 3.2452, 3.1851, 3.0933, 3.0179, 2.9295, 2.8358, 2.7984, 2.7132, 2.7301, 2.6367, 2.5049, 2.5049, 2.5049},
                {2.5049, 2.5049, 2.5049, 2.6367, 2.7421, 2.7689, 2.8377, 2.9000, 2.9852, 3.0802, 3.1621, 3.2342, 3.2588, 3.2935, 3.3161, 3.3587, 3.3980, 3.3980, 3.3587, 3.3161, 3.2935, 3.2588, 3.2342, 3.1621, 3.0802, 2.9852, 2.9000, 2.8377, 2.7689, 2.7421, 2.6367, 2.5049, 2.5049, 2.5049},
                {2.5049, 2.5049, 2.5049, 2.6367, 2.7623, 2.8115, 2.9000, 2.9622, 3.0474, 3.1097, 3.1785, 3.2408, 3.2801, 3.3161, 3.3423, 3.3784, 3.4210, 3.4210, 3.3784, 3.3423, 3.3161, 3.2801, 3.2408, 3.1785, 3.1097, 3.0474, 2.9622, 2.9000, 2.8115, 2.7623, 2.6367, 2.5049, 2.5049, 2.5049},
                {2.5049, 2.5049, 2.5049, 2.6367, 2.7755, 2.8312, 2.9360, 3.0048, 3.0802, 3.1162, 3.1785, 3.2408, 3.2768, 3.3161, 3.3653, 3.4079, 3.4472, 3.4472, 3.4079, 3.3653, 3.3161, 3.2768, 3.2408, 3.1785, 3.1162, 3.0802, 3.0048, 2.9360, 2.8312, 2.7755, 2.6367, 2.5049, 2.5049, 2.5049},
                {2.5167, 2.5167, 2.5167, 2.6492, 2.7886, 2.8606, 2.9622, 3.0333, 3.1228, 3.0999, 3.1719, 3.2342, 3.2768, 3.3227, 3.3718, 3.4144, 3.4603, 3.4603, 3.4144, 3.3718, 3.3227, 3.2768, 3.2342, 3.1719, 3.0999, 3.1228, 3.0333, 2.9622, 2.8606, 2.7886, 2.6492, 2.5167, 2.5167, 2.5167},
                {2.5167, 2.5167, 2.5167, 2.6492, 2.7886, 2.8803, 2.9983, 3.0671, 3.1293, 3.0933, 3.1490, 3.2047, 3.2539, 3.3096, 3.3653, 3.4210, 3.4669, 3.4669, 3.4210, 3.3653, 3.3096, 3.2539, 3.2047, 3.1490, 3.0933, 3.1293, 3.0671, 2.9983, 2.8803, 2.7886, 2.6492, 2.5167, 2.5167, 2.5167},
                {2.5167, 2.5167, 2.5167, 2.6492, 2.7984, 2.8934, 3.0264, 3.0867, 3.1457, 3.0867, 3.1228, 3.1851, 3.2342, 3.3096, 3.3718, 3.4210, 3.4767, 3.4767, 3.4210, 3.3718, 3.3096, 3.2342, 3.1851, 3.1228, 3.0867, 3.1457, 3.0867, 3.0264, 2.8934, 2.7984, 2.6492, 2.5167, 2.5167, 2.5167},
                {2.5167, 2.5167, 2.5167, 2.6492, 2.7984, 2.8934, 3.0264, 3.0867, 3.1457, 3.0867, 3.1228, 3.1851, 3.2342, 3.3096, 3.3718, 3.4210, 3.4767, 3.4767, 3.4210, 3.3718, 3.3096, 3.2342, 3.1851, 3.1228, 3.0867, 3.1457, 3.0867, 3.0264, 2.8934, 2.7984, 2.6492, 2.5167, 2.5167, 2.5167},
                {2.5167, 2.5167, 2.5167, 2.6492, 2.7886, 2.8803, 2.9983, 3.0671, 3.1293, 3.0933, 3.1490, 3.2047, 3.2539, 3.3096, 3.3653, 3.4210, 3.4669, 3.4669, 3.4210, 3.3653, 3.3096, 3.2539, 3.2047, 3.1490, 3.0933, 3.1293, 3.0671, 2.9983, 2.8803, 2.7886, 2.6492, 2.5167, 2.5167, 2.5167},
                {2.5167, 2.5167, 2.5167, 2.6492, 2.7886, 2.8606, 2.9622, 3.0333, 3.1228, 3.0999, 3.1719, 3.2342, 3.2768, 3.3227, 3.3718, 3.4144, 3.4603, 3.4603, 3.4144, 3.3718, 3.3227, 3.2768, 3.2342, 3.1719, 3.0999, 3.1228, 3.0333, 2.9622, 2.8606, 2.7886, 2.6492, 2.5167, 2.5167, 2.5167},
                {2.5049, 2.5049, 2.5049, 2.6367, 2.7755, 2.8312, 2.9360, 3.0048, 3.0802, 3.1162, 3.1785, 3.2408, 3.2768, 3.3161, 3.3653, 3.4079, 3.4472, 3.4472, 3.4079, 3.3653, 3.3161, 3.2768, 3.2408, 3.1785, 3.1162, 3.0802, 3.0048, 2.9360, 2.8312, 2.7755, 2.6367, 2.5049, 2.5049, 2.5049},
                {2.5049, 2.5049, 2.5049, 2.6367, 2.7623, 2.8115, 2.9000, 2.9622, 3.0474, 3.1097, 3.1785, 3.2408, 3.2801, 3.3161, 3.3423, 3.3784, 3.4210, 3.4210, 3.3784, 3.3423, 3.3161, 3.2801, 3.2408, 3.1785, 3.1097, 3.0474, 2.9622, 2.9000, 2.8115, 2.7623, 2.6367, 2.5049, 2.5049, 2.5049},
                {2.5049, 2.5049, 2.5049, 2.6367, 2.7421, 2.7689, 2.8377, 2.9000, 2.9852, 3.0802, 3.1621, 3.2342, 3.2588, 3.2935, 3.3161, 3.3587, 3.3980, 3.3980, 3.3587, 3.3161, 3.2935, 3.2588, 3.2342, 3.1621, 3.0802, 2.9852, 2.9000, 2.8377, 2.7689, 2.7421, 2.6367, 2.5049, 2.5049, 2.5049},
                {2.5049, 2.5049, 2.5049, 2.6367, 2.7301, 2.7132, 2.7984, 2.8358, 2.9295, 3.0179, 3.0933, 3.1851, 3.2452, 3.2615, 3.2702, 3.3227, 3.3522, 3.3522, 3.3227, 3.2702, 3.2615, 3.2452, 3.1851, 3.0933, 3.0179, 2.9295, 2.8358, 2.7984, 2.7132, 2.7301, 2.6367, 2.5049, 2.5049, 2.5049},
                {2.5049, 2.5049, 2.5049, 2.6367, 2.6935, 2.6706, 2.7263, 2.7623, 2.8377, 2.9065, 2.9852, 3.0540, 3.1246, 3.1469, 3.1916, 3.2178, 3.2672, 3.2672, 3.2178, 3.1916, 3.1469, 3.1246, 3.0540, 2.9852, 2.9065, 2.8377, 2.7623, 2.7263, 2.6706, 2.6935, 2.6367, 2.5049, 2.5049, 2.5049},
                {2.5246, 2.5246, 2.5246, 2.6575, 2.6575, 2.6182, 2.6417, 2.6691, 2.7263, 2.7726, 2.8075, 2.8705, 2.9392, 2.9231, 2.9917, 3.0423, 3.0769, 3.0769, 3.0423, 2.9917, 2.9231, 2.9392, 2.8705, 2.8075, 2.7726, 2.7263, 2.6691, 2.6417, 2.6182, 2.6575, 2.6575, 2.5246, 2.5246, 2.5246},
                {2.6122, 2.6122, 2.6122, 2.6122, 2.6122, 2.5690, 2.5719, 2.5887, 2.6444, 2.6707, 2.6903, 2.7402, 2.7682, 2.7334, 2.6701, 2.7518, 2.7525, 2.7525, 2.7518, 2.6701, 2.7334, 2.7682, 2.7402, 2.6903, 2.6707, 2.6444, 2.5887, 2.5719, 2.5690, 2.6122, 2.6122, 2.6122, 2.6122, 2.6122},
                {2.5374, 2.5374, 2.5374, 2.5374, 2.5374, 2.5068, 2.4904, 2.5013, 2.5740, 2.5664, 2.5701, 2.6199, 2.6212, 2.5330, 2.4478, 2.3888, 2.3475, 2.3475, 2.3888, 2.4478, 2.5330, 2.6212, 2.6199, 2.5701, 2.5664, 2.5740, 2.5013, 2.4904, 2.5068, 2.5374, 2.5374, 2.5374, 2.5374, 2.5374},
                {2.4510, 2.4510, 2.4510, 2.4510, 2.4510, 2.4576, 2.4062, 2.4299, 2.4773, 2.4644, 2.4538, 2.4785, 2.4441, 2.3593, 2.2544, 2.1445, 2.0677, 2.0677, 2.1445, 2.2544, 2.3593, 2.4441, 2.4785, 2.4538, 2.4644, 2.4773, 2.4299, 2.4062, 2.4576, 2.4510, 2.4510, 2.4510, 2.4510, 2.4510},
                {2.3822, 2.3822, 2.3822, 2.3822, 2.3822, 2.3822, 2.3691, 2.3411, 2.3757, 2.3366, 2.3486, 2.3167, 2.2805, 2.2086, 2.0808, 1.9792, 1.8743, 1.8743, 1.9792, 2.0808, 2.2086, 2.2805, 2.3167, 2.3486, 2.3366, 2.3757, 2.3411, 2.3691, 2.3822, 2.3822, 2.3822, 2.3822, 2.3822, 2.3822},
                {2.3069, 2.3069, 2.3069, 2.3069, 2.3069, 2.2708, 2.2643, 2.2446, 2.2643, 2.2446, 2.2288, 2.1848, 2.1463, 2.0775, 1.9497, 1.8579, 1.7334, 1.7334, 1.8579, 1.9497, 2.0775, 2.1463, 2.1848, 2.2288, 2.2446, 2.2643, 2.2446, 2.2643, 2.2708, 2.3069, 2.3069, 2.3069, 2.3069, 2.3069},
                {2.2217, 2.2217, 2.2217, 2.2217, 2.2217, 2.1955, 2.1889, 2.1791, 2.1660, 2.1332, 2.1016, 2.0677, 2.0251, 1.9399, 1.8350, 1.7236, 1.6155, 1.6155, 1.7236, 1.8350, 1.9399, 2.0251, 2.0677, 2.1016, 2.1332, 2.1660, 2.1791, 2.1889, 2.1955, 2.2217, 2.2217, 2.2217, 2.2217, 2.2217},
                {2.1332, 2.1332, 2.1332, 2.1332, 2.1332, 2.0972, 2.0972, 2.0840, 2.0546, 2.0349, 2.0050, 1.9595, 1.8842, 1.7990, 1.7039, 1.5860, 1.4746, 1.4746, 1.5860, 1.7039, 1.7990, 1.8842, 1.9595, 2.0050, 2.0349, 2.0546, 2.0840, 2.0972, 2.0972, 2.1332, 2.1332, 2.1332, 2.1332, 2.1332},
                {2.0152, 2.0152, 2.0152, 2.0152, 2.0152, 1.9594, 1.9792, 1.9661, 1.9399, 1.9169, 1.8907, 1.8514, 1.7564, 1.6679, 1.5729, 1.4516, 1.3337, 1.3337, 1.4516, 1.5729, 1.6679, 1.7564, 1.8514, 1.8907, 1.9169, 1.9399, 1.9661, 1.9792, 1.9594, 2.0152, 2.0152, 2.0152, 2.0152, 2.0152},
                {1.9104, 1.9104, 1.9104, 1.9104, 1.9104, 1.8776, 1.8428, 1.8285, 1.8153, 1.7924, 1.7596, 1.7105, 1.6253, 1.5499, 1.4615, 1.3435, 1.2452, 1.2452, 1.3435, 1.4615, 1.5499, 1.6253, 1.7105, 1.7596, 1.7924, 1.8153, 1.8285, 1.8428, 1.8776, 1.9104, 1.9104, 1.9104, 1.9104, 1.9104},
                {1.7531, 1.7531, 1.7531, 1.7531, 1.7531, 1.7302, 1.7105, 1.6976, 1.6690, 1.6482, 1.6187, 1.5925, 1.5008, 1.4287, 1.3533, 1.2354, 1.1469, 1.1469, 1.2354, 1.3533, 1.4287, 1.5008, 1.5925, 1.6187, 1.6482, 1.6690, 1.6976, 1.7105, 1.7302, 1.7531, 1.7531, 1.7531, 1.7531, 1.7531},
                {1.6351, 1.6351, 1.6351, 1.6351, 1.6351, 1.5991, 1.5794, 1.5630, 1.5434, 1.5368, 1.5172, 1.4746, 1.3861, 1.3009, 1.2255, 1.1370, 1.0486, 1.0486, 1.1370, 1.2255, 1.3009, 1.3861, 1.4746, 1.5172, 1.5368, 1.5434, 1.5630, 1.5794, 1.5991, 1.6351, 1.6351, 1.6351, 1.6351, 1.6351},
                {1.5794, 1.5794, 1.5794, 1.5794, 1.5794, 1.5434, 1.4939, 1.4811, 1.4549, 1.4320, 1.4029, 1.3337, 1.2546, 1.1796, 1.1141, 1.0387, 0.9684, 0.9684, 1.0387, 1.1141, 1.1796, 1.2546, 1.3337, 1.4029, 1.4320, 1.4549, 1.4811, 1.4939, 1.5434, 1.5794, 1.5794, 1.5794, 1.5794, 1.5794},
                {1.5368, 1.5368, 1.5368, 1.5368, 1.5368, 1.5172, 1.4868, 1.4624, 1.4121, 1.3861, 1.3411, 1.2681, 1.1838, 1.1141, 1.0617, 0.9634, 0.9059, 0.9059, 0.9634, 1.0617, 1.1141, 1.1838, 1.2681, 1.3411, 1.3861, 1.4121, 1.4624, 1.4868, 1.5172, 1.5368, 1.5368, 1.5368, 1.5368, 1.5368},
            };

            #endregion

            int s32_Cols = s32_Values.GetLength(0);
            int s32_Rows = s32_Values.GetLength(1);

            // In Line mode the pen is used to draw the polygon border lines. The color is assigned from the ColorScheme.
            // In Fill mode the pen is used to draw the thin separator lines (always 1 pixel, black)
            Pen? i_Pen = (e_Mode == ePolygonMode.Fill) ? new Pen(Color.Black, 2) : null;

            cColorScheme i_Scheme = new cColorScheme(Plot3D.Editor3D.eColorScheme.Monochrome);
            cSurfaceData i_Data = new cSurfaceData(e_Mode, s32_Cols, s32_Rows, i_Pen, i_Scheme);
            cSurfaceData i_Data2 = new cSurfaceData(e_Mode, s32_Cols, s32_Rows, i_Pen, i_Scheme);

            //cSurfaceData planform = new cSurfaceData(e_Mode, 4, planformCurvePoints_LE[0].Count * 2, i_Pen, i_Scheme);

            //for (int i = 0; i < planformCurvePoints_LE.Count; i++)
            //{
            //    for (int j = planformCurvePoints_LE[i].Count; j > 0; j--)
            //    {
            //        double d_X = planformCurvePoints_LE[i][j].X;
            //        double d_Y = planformCurvePoints_LE[i][j].Y;
            //        double d_Z = 0;

            //        //String s_Tooltip = String.Format("Col = {0}\nRow = {1}\nRaw Value = {2}", d_X, d_Y, d_Z);
            //        cPoint3D i_Point = new cPoint3D(d_X, d_Y, d_Z, null, d_Z);
            //        //cPoint3D i_Point2 = new cPoint3D(d_X, d_Y, d_Z + 0.1, null, d_Z);

            //        planform.SetPointAt(i*(j+1), j, i_Point);
            //        //planform.SetPointAt(i, j, i_Point2);
            //    }
            //}

            for (int C = 0; C < i_Data.Cols; C++)
            {
                for (int R = 0; R < i_Data.Rows; R++)
                {
                    double s32_RawValue = s32_Values[C, R];

                    double d_X = C;
                    double d_Y = R;
                    double d_Z = (s32_RawValue - 0.9059) * 5;

                    String s_Tooltip = String.Format("Col = {0}\nRow = {1}\nRaw Value = {2}", C, R, s32_RawValue);
                    cPoint3D i_Point = new cPoint3D(d_X, d_Y, d_Z, s_Tooltip, s32_RawValue);
                    cPoint3D i_Point2 = new cPoint3D(d_X, d_Y, -d_Z, s_Tooltip, s32_RawValue);

                    i_Data.SetPointAt(C, R, i_Point);
                    i_Data2.SetPointAt(C, R, i_Point2);
                }
            }

            Editor3d.Clear();
            Editor3d.Normalize = eNormalize.MaintainXYZ;
            Editor3d.AddRenderData(i_Data);
            Editor3d.AddRenderData(i_Data2);

            //Editor3d.AddRenderData(planform);

            //editor3d1.Selection.Callback = OnSelectEvent;
            Editor3d.Selection.HighlightColor = Color.FromArgb(100, 100, 100);
            Editor3d.Selection.MultiSelect = true;
            //editor3d1.Selection.Enabled = true;
            Editor3d.Recalculate(true);
            Editor3d.TooltipMode = eTooltip.Off;

            // Single point selection works only in Fill mode
            //if (e_Mode == ePolygonMode.Lines)
            //checkPointSelection.Enabled = false;

            // FIRST: Adjust Selection.SinglePoints which will remove all current selections
            //checkPointSelection.Checked = (e_Mode == ePolygonMode.Lines);

            // AFTER: Pre-select one polygon
            //cPolygon3D i_Polygon = i_Data.GetPolygonAt(10, 5);

            //if (editor3D.Selection.SinglePoints) // Select the 4 corner points
            //{
            //    foreach (cPoint3D i_Point in i_Polygon.Points)
            //    {
            //        i_Point.Selected = true;
            //    }
            //}
            //else // Select the polygon itself
            //{
            //    i_Polygon.Selected = true;
            //}
        }

        private void Form1_SizeChanged(object? sender, EventArgs? e)
        {
            tabControl1.Height = this.ClientSize.Height /*- editor3d1.Top * 2*/;
            tabControl1.Width = this.ClientSize.Width /*- editor3d1.Left * 2*/;

            Editor3d.Height = tab3DView.Height - Editor3d.Top * 2;
            Editor3d.Width = tab3DView.Width - Editor3d.Left * 2;

            formsPlot1.Height = tabPlanformView.Height - formsPlot1.Top * 2;
            formsPlot1.Width = tabPlanformView.Width - formsPlot1.Left * 2;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DemoSurface(ePolygonMode.Fill);
            Form1_SizeChanged(sender: null, e: null);
        }


    }
}
