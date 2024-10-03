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
using Tekla.Structures.Model.UI;
using System.Collections;
using Point = Tekla.Structures.Geometry3d.Point;
using LineSegment = Tekla.Structures.Geometry3d.LineSegment;
using Color = Tekla.Structures.Model.UI.Color;
using Vector = Tekla.Structures.Geometry3d.Vector;
using static Tekla.Structures.Model.Position;
using System.Reflection;
using System.CodeDom;


namespace DoubleSidedGirtConn
{
    public partial class GirtConnection : Form
    {
        #region Global Coordinates
        Model myModel = new Model();    //  Model
        Picker pick = new Picker();     //  Picker

        double plateThickness = 12.7; //  plate
        double plateWidth = 101.6; //  plate

        private static ArrayList ObjectList = new ArrayList(); // weld

        #endregion
        public GirtConnection()
        {
            InitializeComponent();
        }
        private void GirtConnection_Load(object sender, EventArgs e)
        {
            if (!myModel.GetConnectionStatus())
            {
                MessageBox.Show("Connection to Tekla Structures failed.");
            }
        }
        #region Double Girt Connection

        #region SupportMethods
        #endregion

        private void btn_GirtConn_Click(object sender, EventArgs e)
        {
            #region PICK Parts
            ModelObject column = pick.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick Primary Part");
            ModelObject girt1 = pick.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick Secondary Part 1");
            ModelObject girt2 = pick.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick Secondary Part 2");
            #endregion

            #region Error Check (Girt Elevation Level)
            Beam girtPart1 = girt1 as Beam;
            Beam girtPart2 = girt2 as Beam;
            Beam col = column as Beam;

            Point girt1Point;
            Point girt2Point;

            if (Distance.PointToPoint(girtPart1.StartPoint, col.EndPoint) < Distance.PointToPoint(girtPart1.EndPoint, col.EndPoint))
            {
                girt1Point = girtPart1.StartPoint;
            }
            else
            {
                girt1Point = girtPart1.EndPoint;
            }

            if (Distance.PointToPoint(girtPart2.StartPoint, col.EndPoint) < Distance.PointToPoint(girtPart2.EndPoint, col.EndPoint))
            {
                girt2Point = girtPart2.StartPoint;
            }
            else
            {
                girt2Point = girtPart2.EndPoint;
            }

            if (girt1Point.Z == girt2Point.Z)
            {
                MessageBox.Show("The elevation of the two girts is same");
                #endregion

                #region Coordinate System & WorkPlane
                var colPart = column as Beam;
                var g1 = girt1 as Beam;
                var g2 = girt2 as Beam;

                Point origin1 = girt1Point;

                try
                {
                    if (origin1 == null)
                    {
                        MessageBox.Show("No point selected. Coordinate System creation aborted");
                        return;
                    }
                    var girtCoord = (girt1 as Part).GetCoordinateSystem();
                    girtCoord.Origin = origin1;
                    //girtCoord.AxisX = girtCoord.AxisX *- 1;
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
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred");
                }
                #endregion

                #region Girt CUTs
                CutPlane cutPlane1 = new CutPlane();
                CutPlane cutPlane2 = new CutPlane();
                Beam girt1Part = girt1 as Beam;
                Beam girt2Part = girt2 as Beam;

                cutPlane1.Plane = new Plane();
                cutPlane1.Plane.Origin = new Point(71.4, 0, 0);
                cutPlane1.Plane.AxisX = new Vector(0, 71.4, 0);
                cutPlane1.Plane.AxisY = new Vector(0, 0, -100);
                cutPlane1.Father = girtPart1;
                cutPlane1.Insert();

                cutPlane2.Plane = new Plane();
                cutPlane2.Plane.Origin = new Point(-71.4, 0, 0);
                cutPlane2.Plane.AxisX = new Vector(0, -71.4, -100);
                cutPlane2.Plane.AxisY = new Vector(0, 0, -100);
                cutPlane2.Father = girtPart2;
                cutPlane2.Insert();
                #endregion

                #region PLATEs
                Beam plate1 = new Beam();
                plate1.Profile.ProfileString = $"PL{101.6}*{12.7}" /*"PL4\"*1/4"*/;
                plate1.Material.MaterialString = "A992";
                plate1.Class = "1";
                plate1.Position.Depth = DepthEnum.BEHIND;
                plate1.StartPoint = new Point(3.97, 0, -2.38);
                plate1.EndPoint = new Point(150, 0, -2.38);
                plate1.Position.Rotation = RotationEnum.FRONT;
                plate1.Insert();

                Beam plate2 = new Beam();
                plate2.Profile.ProfileString = $"PL{101.6}*{12.7}" ;
                plate2.Material.MaterialString = "A992";
                plate2.Class = "1";
                plate2.Position.Depth = DepthEnum.BEHIND;
                plate2.StartPoint = new Point(-3.97, 0, -2.38);
                plate2.EndPoint = new Point(-150, 0, -2.38);
                plate2.Position.Rotation = RotationEnum.FRONT;
                plate2.Insert();

                #endregion

                #region Plates WELDs to Column
                Weld weld1 = new Weld();
                ObjectList.Add(colPart);
                ObjectList.Add(plate1);
                weld1.Delete();
                weld1.MainObject = colPart;
                weld1.SecondaryObject = plate1;
                weld1.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld1.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld1.SizeAbove = 12.7;
                weld1.SizeBelow = 12.7;
                weld1.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld1.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld1.AroundWeld = false;
                weld1.ShopWeld = false;
                if (!weld1.Insert())
                {
                    MessageBox.Show("Weld insert failed");
                }

                Weld weld2 = new Weld();
                ObjectList.Add(colPart);
                ObjectList.Add(plate1);
                weld2.Delete();
                weld2.MainObject = colPart;
                weld2.SecondaryObject = plate2;
                weld2.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld2.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld2.SizeAbove = 12.7;
                weld2.SizeBelow = 12.7;
                weld2.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld2.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld2.AroundWeld = false;
                weld2.ShopWeld = false;
                if (!weld2.Insert())
                {
                    MessageBox.Show("Weld insert failed");
                }
                #endregion

                #region Plates BOLTING to Girts
                Beam plate1_Part = plate1 as Beam;
                Point plate1StartPoint = plate1_Part.StartPoint;
                Part secondaryPart1 = girtPart1;
                BoltArray bArr1 = new BoltArray();
                bArr1.Delete();
                bArr1.PartToBoltTo = plate1_Part;
                bArr1.PartToBeBolted = secondaryPart1;
                bArr1.FirstPosition = plate1StartPoint;
                bArr1.SecondPosition = plate1_Part.EndPoint;
                bArr1.BoltSize = 12.7;
                bArr1.Tolerance = 2.00;
                bArr1.BoltStandard = "A325N";
                bArr1.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
                bArr1.Length = 80;
                bArr1.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;
                bArr1.Position.Depth = Position.DepthEnum.MIDDLE;
                bArr1.Position.Plane = Position.PlaneEnum.MIDDLE;
                bArr1.Position.Rotation = Position.RotationEnum.FRONT;

                bArr1.AddBoltDistX(0);
                bArr1.StartPointOffset.Dx = 114.3;
                bArr1.AddBoltDistY(50.8);
                //bArr.Insert();
                if (!bArr1.Insert())
                {
                    MessageBox.Show("BoltArray Insert Failed");
                }

                Beam plate2_Part = plate2 as Beam;
                Point plate2StartPoint = plate2_Part.StartPoint;
                Part secondaryPart2 = girtPart2;
                BoltArray bArr2 = new BoltArray();
                bArr2.Delete();
                bArr2.PartToBoltTo = plate2_Part;
                bArr2.PartToBeBolted = secondaryPart2;
                bArr2.FirstPosition = plate2StartPoint;
                bArr2.SecondPosition = plate2_Part.EndPoint;
                bArr2.BoltSize = 12.7;
                bArr2.Tolerance = 2.00;
                bArr2.BoltStandard = "A325N";
                bArr2.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
                bArr2.Length = 80;
                bArr2.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;
                bArr2.Position.Depth = Position.DepthEnum.MIDDLE;
                bArr2.Position.Plane = Position.PlaneEnum.MIDDLE;
                bArr2.Position.Rotation = Position.RotationEnum.FRONT;

                bArr2.AddBoltDistX(0);
                bArr2.StartPointOffset.Dx = 114.3;
                bArr2.AddBoltDistY(50.8);
                //bArr.Insert();
                if (!bArr2.Insert())
                {
                    MessageBox.Show("BoltArray Insert Failed");
                }

                #endregion
            }
            else
            {
                MessageBox.Show("The elevation of the two girts is not same");
                return;
            }

        }
        #endregion
    }
}
