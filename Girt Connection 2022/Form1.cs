using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Tekla.Structures;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Point = Tekla.Structures.Geometry3d.Point;
using Tekla.Structures.Model.UI;
using Color = Tekla.Structures.Model.UI.Color;
using Tekla.Structures.ModelInternal;
using System.Collections;

namespace Girt_Connection_2022
{
    public partial class Form1 : Form
    {
        #region Global Variables
        Model myModel = new Model();
        Picker pick = new Picker();
        ControlPoint cp = new ControlPoint(new Point (0, 0, 0));//cp.Insert();
        private static ArrayList ObjectList = new ArrayList();
        #endregion
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!myModel.GetConnectionStatus())
            {
                MessageBox.Show("Tekla Structures is NOT connected");
            }
        }

        #region Support Methods
        private double inch2MM(double inch) //  converts mm to inch
        {
            return (inch * 25.4);
        }
        //private static void DrawCoordinateSystem(CoordinateSystem CoordinateSystem)
        //{
        //    DrawVector(CoordinateSystem.Origin, CoordinateSystem.AxisX, "X");
        //    DrawVector(CoordinateSystem.Origin, CoordinateSystem.AxisY, "Y");
        //}
        #endregion

        #region Girt Connection //  W8X24 & Z4"*3"*1/4
        private void btn_GirtConn_Click(object sender, EventArgs e)
        {
            #region Pick Parts
            ModelObject columnObject = pick.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick Primary Part");
            ModelObject girtObject = pick.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick Secondary Part");
            #endregion
            cp.Insert();
            #region Coordinate System & WorkPlane
            var girt = girtObject as Beam;
            var column = columnObject as Beam;
            Point origin1;
            if (Distance.PointToPoint(girt.StartPoint, column.EndPoint) < Distance.PointToPoint(girt.EndPoint, column.EndPoint))
            {
                origin1 = girt.StartPoint;
                try
                {
                    if (origin1 == null)
                    {
                        MessageBox.Show("No point selected for origin, cannot create Coordinate System.");
                        return;
                    }
                    var girtCoord = girt.GetCoordinateSystem();
                    girtCoord.Origin = origin1;
                    var workPlaneHandler = new Model().GetWorkPlaneHandler();
                    var newWorkPlane = new TransformationPlane(girtCoord);

                    new Model().CommitChanges();

                    if (girtCoord == null)
                        throw new ArgumentNullException(nameof(girtCoord));

                    var gd = new GraphicsDrawer();
                    var origin = girtCoord.Origin;

                    var xPoint = origin + 400 * girtCoord.AxisX.GetNormal();
                    var yPoint = origin + 400 * girtCoord.AxisY.GetNormal();
                    var zPoint = origin + 400 * girtCoord.AxisX.Cross(girtCoord.AxisY).GetNormal();

                    var xColor = new Color(1, 0, 0);
                    var yColor = new Color(0, 1, 0);
                    var zColor = new Color(0, 0, 1);

                    gd.DrawLineSegment(origin, xPoint, xColor);
                    gd.DrawText(xPoint, "X", xColor);
                    gd.DrawLineSegment(origin, yPoint, yColor);
                    gd.DrawText(yPoint, "Y", yColor);
                    gd.DrawLineSegment(origin, zPoint, zColor);
                    gd.DrawText(zPoint, "Z", zColor);

                    if (!string.IsNullOrWhiteSpace("ORIGIN"))
                        gd.DrawText(origin, "ORIGIN", new Color(0, 0, 0));

                    if (!workPlaneHandler.SetCurrentTransformationPlane(newWorkPlane))
                    {
                        MessageBox.Show("Coordinate System failed to insert");
                        myModel.CommitChanges();
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("An error occurred.");
                }
                
                #region Girt Cut
                CutPlane cut = new CutPlane();
                cut.Plane = new Plane();
                cut.Plane.Origin = new Point(inch2MM(4), 0, 0);
                cut.Plane.AxisX = new Vector(0, inch2MM(4), 0);
                cut.Plane.AxisY = new Vector(0, 0, inch2MM(-4));
                cut.Father = girt;
                cut.Insert();
                #endregion

                #region Plate
                Point plStartPoint = new Point(inch2MM(0.125), 0, inch2MM(-0.125));
                Point plEndPoint = new Point(inch2MM(7), 0, inch2MM(-0.125));
                Beam cPlate = new Beam(plStartPoint, plEndPoint);
                string plateProfile = "PL";
                string plateWidth = "76.2";     // 3 inches
                string plateThickness = "12.7"; // 1/2 inch
//                cPlate.Profile.ProfileString = "PL inch2MM(3) * inch2MM(1/2)";
                cPlate.Profile.ProfileString = $"{plateProfile} {plateWidth}*{plateThickness}";
                cPlate.Material.MaterialString = "A153";
                cPlate.Class = "4";
                cPlate.Position.Plane = Position.PlaneEnum.MIDDLE;
                cPlate.Position.Rotation = Position.RotationEnum.FRONT;
                cPlate.Insert();
                #endregion

                #region Weld
                Weld weld1 = new Weld();
                Weld weld2 = new Weld();
                ObjectList.Add(girt);
                ObjectList.Add(cPlate);
                ObjectList.Add(column);
                weld1.Delete();
                weld1.MainObject = column;
                weld1.SecondaryObject = cPlate;
                weld1.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld1.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld1.SizeAbove = inch2MM(0.5);
                weld1.SizeBelow = inch2MM(0.5);
                weld1.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld1.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld1.AroundWeld = false;
                weld1.ShopWeld = false;
                if (!weld1.Insert())
                {
                    MessageBox.Show("Weld insert failed");
                }

                weld2.Delete();
                weld2.MainObject = cPlate;
                weld2.SecondaryObject = girt;
                //weld2.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld2.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                //weld2.SizeAbove = inch2MM(0.5);
                weld2.SizeBelow = inch2MM(0.5);
                weld2.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld2.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld2.AroundWeld = false;
                weld2.ShopWeld = false;
                if (!weld2.Insert())
                {
                    MessageBox.Show("Weld insert failed");
                }
                #endregion

                #region Bolt Array
//                Point plStrtPt = cPlate.StartPoint;
                Part secondaryPart = cPlate;
                Part primaryPart = girt;
                BoltArray bArr = new BoltArray();
                bArr.Delete();
                bArr.PartToBoltTo = primaryPart;
                bArr.PartToBeBolted = secondaryPart;
                bArr.FirstPosition = cPlate.StartPoint;
                bArr.SecondPosition = cPlate.EndPoint;
                bArr.BoltSize = inch2MM(0.5);
                bArr.Tolerance = inch2MM(0.079);
                bArr.BoltStandard = "A325N";
                bArr.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
                bArr.Length = 80;
                bArr.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;
                bArr.Position.Depth = Position.DepthEnum.MIDDLE;
                bArr.Position.Plane = Position.PlaneEnum.MIDDLE;
                bArr.Position.Rotation = Position.RotationEnum.FRONT;

                bArr.AddBoltDistX(0);
                bArr.StartPointOffset.Dx = inch2MM(5.375);
                bArr.AddBoltDistY(inch2MM(1.5));
                //bArr.Insert();
                if (!bArr.Insert())
                {
                    MessageBox.Show("BoltArray Insert Failed");
                }
                #endregion
            }
            else
            {
                    origin1 = girt.EndPoint;
                    try
                    {
                        if (origin1 == null)
                        {
                            MessageBox.Show("No point selected. Coordinate System creation aborted");
                            return;
                        }
                        var girtCoord = (girt as Part).GetCoordinateSystem();
                        girtCoord.Origin = origin1;
                        girtCoord.AxisX = girtCoord.AxisX * -1;
                        girtCoord.AxisY = girtCoord.AxisY * -1;
                        var workPlaneHandler = new Model().GetWorkPlaneHandler();
                        var newWorkPlane = new TransformationPlane(girtCoord);

                        new Model().CommitChanges();

                        if (girtCoord == null)
                            throw new ArgumentNullException(nameof(girtCoord));

                        var gd = new GraphicsDrawer();
                        var origin = girtCoord.Origin;

                        var xPoint = origin + 400 * girtCoord.AxisX.GetNormal();
                        var yPoint = origin + 400 * girtCoord.AxisY.Cross(girtCoord.AxisX).GetNormal();
                        var zPoint = origin + 400 * girtCoord.AxisY.GetNormal();

                        var xColor = new Color(1, 0, 0);
                        var yColor = new Color(0, 1, 0);
                        var zColor = new Color(0, 0, 1);

                        gd.DrawLineSegment(origin, xPoint, xColor);
                        gd.DrawText(xPoint, "X", xColor);
                        gd.DrawLineSegment(origin, yPoint, yColor);
                        gd.DrawText(yPoint, "Y", yColor);
                        gd.DrawLineSegment(origin, zPoint, zColor);
                        gd.DrawText(zPoint, "Z", zColor);

                        if (!string.IsNullOrWhiteSpace("ORIGIN"))
                            gd.DrawText(origin, "ORIGIN", new Color(0, 0, 0));

                        if (!workPlaneHandler.SetCurrentTransformationPlane(newWorkPlane))
                        {
                            MessageBox.Show("Coordinate System failed to insert");
                            myModel.CommitChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred");
                    }
                #region Girt Cut
                CutPlane cut = new CutPlane();
                cut.Plane = new Plane();
                cut.Plane.Origin = new Point(inch2MM(4), 0, 0);
                cut.Plane.AxisX = new Vector(0, inch2MM(4), 0);
                cut.Plane.AxisY = new Vector(0, 0, inch2MM(-4));
                cut.Father = girt;
                cut.Insert();
                #endregion

                #region Plate
                Point plStartPoint = new Point(inch2MM(0.125), 0, inch2MM(-0.125));
                Point plEndPoint = new Point(inch2MM(7), 0, inch2MM(-0.125));
                Beam cPlate = new Beam(plStartPoint, plEndPoint);
                string plateProfile = "PL";
                string plateWidth = "76.2";     // 3 inches
                string plateThickness = "12.7"; // 1/2 inch
                                                //                cPlate.Profile.ProfileString = "PL inch2MM(3) * inch2MM(1/2)";
                cPlate.Profile.ProfileString = $"{plateProfile} {plateWidth}*{plateThickness}";
                cPlate.Material.MaterialString = "A153";
                cPlate.Class = "4";
                cPlate.Position.Plane = Position.PlaneEnum.MIDDLE;
                cPlate.Position.Rotation = Position.RotationEnum.FRONT;
                cPlate.Insert();
                #endregion

                #region Weld
                Weld weld1 = new Weld();
                Weld weld2 = new Weld();
                ObjectList.Add(girt);
                ObjectList.Add(cPlate);
                ObjectList.Add(column);
                weld1.Delete();
                weld1.MainObject = column;
                weld1.SecondaryObject = cPlate;
                weld1.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld1.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld1.SizeAbove = inch2MM(0.5);
                weld1.SizeBelow = inch2MM(0.5);
                weld1.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld1.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld1.AroundWeld = false;
                weld1.ShopWeld = false;
                if (!weld1.Insert())
                {
                    MessageBox.Show("Weld insert failed");
                }

                weld2.Delete();
                weld2.MainObject = cPlate;
                weld2.SecondaryObject = girt;
                //weld2.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld2.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                //weld2.SizeAbove = inch2MM(0.5);
                weld2.SizeBelow = inch2MM(0.5);
                weld2.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld2.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld2.AroundWeld = false;
                weld2.ShopWeld = false;
                if (!weld2.Insert())
                {
                    MessageBox.Show("Weld insert failed");
                }
                #endregion

                #region Bolt Array
                //                Point plStrtPt = cPlate.StartPoint;
                Part secondaryPart = cPlate;
                Part primaryPart = girt;
                BoltArray bArr = new BoltArray();
                bArr.Delete();
                bArr.PartToBoltTo = primaryPart;
                bArr.PartToBeBolted = secondaryPart;
                bArr.FirstPosition = cPlate.StartPoint;
                bArr.SecondPosition = cPlate.EndPoint;
                bArr.BoltSize = inch2MM(0.5);
                bArr.Tolerance = inch2MM(0.079);
                bArr.BoltStandard = "A325N";
                bArr.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
                bArr.Length = 80;
                bArr.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;
                bArr.Position.Depth = Position.DepthEnum.MIDDLE;
                bArr.Position.Plane = Position.PlaneEnum.MIDDLE;
                bArr.Position.Rotation = Position.RotationEnum.FRONT;

                bArr.AddBoltDistX(0);
                bArr.StartPointOffset.Dx = inch2MM(5.375);
                bArr.AddBoltDistY(inch2MM(1.5));
                //bArr.Insert();
                if (!bArr.Insert())
                {
                    MessageBox.Show("BoltArray Insert Failed");
                }
                #endregion
            }
            #endregion
        }
        #endregion
    }
}
// 700 sq ft > bhiwandi || 1.5L / 1.75L Guntha