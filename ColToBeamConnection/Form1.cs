using System;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Point = Tekla.Structures.Geometry3d.Point;
using Tekla.Structures.Model.UI;
using System.Collections;
using System.Collections.Generic;
using Tekla.Structures.Geometry3d;
using Color = Tekla.Structures.Model.UI.Color;
using Tekla.Structures.ModelInternal;
using static Tekla.Structures.Model.Position;
using System.Reflection.Emit;

namespace ColToBeamConnection
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Model myModel = new Model();    //  body
        Picker _pick = new Picker();    //  body

        Beam myCol;     //  column
        Beam myBeam;    //  beam

        double plateThickness = 10; //  plate

        private static ArrayList ObjectListw = new ArrayList(); // weld
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!myModel.GetConnectionStatus())
            {
                MessageBox.Show("Tekla Structures not connected");
                return;
            }
        }

        #region COLUMN
        private void btn_insertCol_Click(object sender, EventArgs e)
        {
            Point startPoint = _pick.PickPoint("Pick the point of column");
            Point endPoint = new Point(startPoint.X, startPoint.Y, 4990.46);
            myCol = new Beam(startPoint, endPoint);
            myCol.Profile.ProfileString = "W16X40";
            myCol.Material.MaterialString = "A992";
            myCol.Class = "2";
            myCol.Insert();
            myModel.CommitChanges();
        }
        #endregion

        #region BEAM
        private void btn_insertBeam_Click(object sender, EventArgs e)
        {
            Point startPoint = _pick.PickPoint("Pick the start point of the beam");
            Point endPoint = _pick.PickPoint("Pick the end point of the beam");
            myBeam = new Beam(startPoint, endPoint);
            myBeam.Profile.ProfileString = "W16X40";
            myBeam.Material.MaterialString = "A992";
            myBeam.Class = "4";
            myBeam.Insert();

            myModel.CommitChanges();
        }
        #endregion

        #region PLATE
        private void btn_Plate_Click(object sender, EventArgs e)
        {
            ArrayList pickedPoints = _pick.PickPoints(Picker.PickPointEnum.PICK_POLYGON, "Pick Polygon Points");
            ArrayList platePoints = new ArrayList();
            foreach (Point p in pickedPoints)
            {
                ContourPoint cPoints = new ContourPoint(p, new Chamfer());
                platePoints.Add(cPoints);
            }
            ContourPlate cp = new ContourPlate();
            cp.Contour.ContourPoints = platePoints;
            cp.Profile.ProfileString = "10";
            cp.Material.MaterialString = "1500";
            cp.Class = "4";
            cp.Position.Depth = Position.DepthEnum.BEHIND;
            cp.Insert();

            myModel.CommitChanges();
        }
        #endregion

        #region CONNECTION
        private void btn_Connection_Click(object sender, EventArgs e)
        {
            #region Pick 2 Parts
            ModelObject col = _pick.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick primary part");
            ModelObject beam = _pick.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick secondary part");
            #endregion

            #region CoordinateSystem
            var beamPart = beam as Beam;
            Point origin11 = beamPart.StartPoint;

            try
            {
                if (origin11 == null)
                {
                    MessageBox.Show("No point selected. Coordinate System creation aborted");
                    return;
                }

//                var colCoord = (col as Part).GetCoordinateSystem();
                var beamCoord = (beam as Part).GetCoordinateSystem();
                var workPlaneHandler = new Model().GetWorkPlaneHandler();
                var newWorkPlane = new TransformationPlane(beamCoord);

                new Model().CommitChanges();

                if (beamCoord == null)
                    throw new ArgumentNullException(nameof(beamCoord));
                
                var gd = new GraphicsDrawer();

                var origin = beamCoord.Origin;
                var xPoint = origin + 400 * beamCoord.AxisX.GetNormal();
                var yPoint = origin + 400 * beamCoord.AxisY.Cross(beamCoord.AxisX).GetNormal();
                var zPoint = origin + 400 * beamCoord.AxisY.GetNormal(); 

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

            #region CUT
            CutPlane cutPlane = new CutPlane();
            Beam beamStart = beam as Beam;
            //var yCoor = Convert.ToDouble(columnEnd.EndPoint);
            cutPlane.Plane = new Plane();
            cutPlane.Plane.Origin = new Point(10, 0, 0);
            cutPlane.Plane.AxisX = new Vector(0, 1000, 0);
            cutPlane.Plane.AxisY = new Vector(0, 0, -1000);
            cutPlane.Father = beamPart;
            cutPlane.Insert();
            #endregion

            #region PLATE
            ContourPlate cp = new ContourPlate();
            //Point beamSrtPt = beamPart.StartPoint;
            cp.AddContourPoint(new ContourPoint(new Point(0, -209.55, 88.9), null));
            cp.AddContourPoint(new ContourPoint(new Point(0, 209.55, 88.9), null));
            cp.AddContourPoint(new ContourPoint(new Point(0, 209.55, -88.9), null));
            cp.AddContourPoint(new ContourPoint(new Point(0, -209.55, -88.9), null));
            cp.Profile.ProfileString = plateThickness.ToString();
            cp.Material.MaterialString = "43A";
            cp.Class = "1";
            cp.Position.Depth = DepthEnum.BEHIND;
            cp.Insert();
            #endregion

            #region WELD
            Weld weld = new Weld();
            ObjectListw.Add(beamPart);
            ObjectListw.Add(cp);
            weld.Delete();
            weld.MainObject = beamPart;
            weld.SecondaryObject = cp;
            weld.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld.SizeAbove = 10;
            weld.SizeBelow = 10;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
            weld.AroundWeld = false;
            weld.ShopWeld = false;
            if (!weld.Insert())
            {
                MessageBox.Show("Weld insert failed");
            }
            #endregion

            #region BoltArray
            Part primaryCol = col as Part;
            Beam beam_Part = beam as Beam;
            Point beamSrtPt = beam_Part.StartPoint;
            Part secondaryPlate = cp;
            BoltArray bArr = new BoltArray();
            bArr.Delete();
            bArr.PartToBoltTo = primaryCol;
            bArr.PartToBeBolted = secondaryPlate;
            //bArr.FirstPosition = origin11;
            bArr.FirstPosition = beamSrtPt;
            bArr.SecondPosition = new Point(0, -406.4, 0 );
            bArr.BoltSize = 25.05;
            bArr.Tolerance = 2.00;
            bArr.BoltStandard = "A325N";
            bArr.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
            bArr.Length = 80;
            bArr.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;
            bArr.Position.Depth = Position.DepthEnum.MIDDLE;
            bArr.Position.Plane = Position.PlaneEnum.MIDDLE;
            bArr.Position.Rotation = Position.RotationEnum.BELOW;

            bArr.AddBoltDistX(88.9);
            bArr.AddBoltDistX(88.9);
            bArr.AddBoltDistX(88.9);
            bArr.StartPointOffset.Dx = 63.5;
            bArr.AddBoltDistY(88.9);
            bArr.Insert();
            if (!bArr.Insert())
            {
                MessageBox.Show("BoltArray Insert Failed");
            }
            #endregion

            myModel.CommitChanges();
        }
        #endregion

        
    }
}