using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model.UI;
using Point = Tekla.Structures.Geometry3d.Point;
using Color = Tekla.Structures.Model.UI.Color;
using static Tekla.Structures.Model.Position;


namespace ParallelBeamConnection_Splice
{
    public partial class ParallelBeamConnection_Splice : Form
    {
        Model myModel = new Model();
        Picker _pick = new Picker();
        const double inch = 25.4;
        double plateLength = 6 * inch;
        double plateThickness = 0.5 * inch;
        double beamDepth = 0.0;
        double boltSize = 12.7;
        double tolerence = 2.00;
        string boltStandard = "A325N";
        double cutLength = 80;
        double Length = 80;
        double extraLength = 0;

        public ParallelBeamConnection_Splice()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        #region SPLICE CONN
        private void btn_SpliceConnection_Click(object sender, EventArgs e)
        {
            TransformationPlane orginalTP = SetGlobalAndStoreOriginal();    //saving current TransformationPlane and setting new    // variable 'originalTP' of type 'TransformationPlane'

            CoordinateSystem cso = new CoordinateSystem();      //  creates a new coordinate system
            (Beam beam1, Beam beam2) pick = Pick2Beam();       //  pick two beams from the model and returning them as a Tuple

            GeometricPlane beam1GP = GetGeometricPlane(pick.beam1);  //  get the Geometric Planes of both beams
            GeometricPlane beam2GP = GetGeometricPlane(pick.beam2);  //  GetGeometryPlane retrieves/calculates a GeometricPlane associated with the Beam object provided

            bool isParrallel = Tekla.Structures.Geometry3d.Parallel.PlaneToPlane(beam1GP, beam2GP); //check if the planes of the two beams are parallel or not
            if (isParrallel)
            {
                try
                {
                    ArrayList beam1CL = pick.beam1.GetCenterLine(false);    // get the centre line of beam 1
                    ArrayList beam2CL = pick.beam2.GetCenterLine(false);    // get the centre line of beam 2

                    Point midBeam1 = GetMidPoint(beam1CL[0] as Point, beam1CL[1] as Point); //calculate the midpoints of the center lines of both beams
                    Point midBeam2 = GetMidPoint(beam2CL[0] as Point, beam2CL[1] as Point); // and store in Point midBeam1 & midBeam2

                    Point originPoint = GetMidPoint(midBeam1, midBeam2);    //calculate the midpoint between two midpoints to use as the origin

                    //define a new coordinate system at the origin point with X & Y axes from first beam
                    CoordinateSystem cs = new CoordinateSystem(originPoint, pick.beam1.GetCoordinateSystem().AxisX.GetNormal(), pick.beam1.GetCoordinateSystem().AxisY.GetNormal());

                    //create & set a new transformation plane using the defined coordinate system
                    TransformationPlane transformationPlane = new TransformationPlane(cs);
                    myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(transformationPlane);

                    CoordinateSystem setCoor = new CoordinateSystem();

                    //draw the coordinate system for debugging process
                    ViewHandler.RedrawWorkplane();
                    DrawCoordinateSystem(new CoordinateSystem(), "Org");

                    //inserting the fittings on both beams at the defined coordinate system
                    InsertFitting(pick.beam1, setCoor);
                    InsertFitting(pick.beam2, setCoor);

                    //inserting web plates and flange plates on the beams
                    Beam webPlate = InsertWebPlate(pick.beam1, setCoor);
                    (Beam beamTP, Beam beamBP) flangePlate = InsertFlangePlates(pick.beam1, setCoor);

                    //transform the midpoints into the local coordinate system of the new transformation plane
                    TransformationPlane transformationPlane1 = new TransformationPlane(setCoor);
                    Matrix matrix = transformationPlane1.TransformationMatrixToLocal;

                    var trMidBeam1 = matrix.Transform(new Point(midBeam1));
                    var trMidBeam2 = matrix.Transform(new Point(midBeam2));

                    //draw text at the transformed midpoints for debugging purpose
                    new GraphicsDrawer().DrawText(trMidBeam1, "b1p", new Color());
                    new GraphicsDrawer().DrawText(trMidBeam2, "b2p", new Color());

                    //calculcate the direction vectors for placing bolts on the web plate
                    var directionVectorToMovePointToDesiredLocation = new Vector(trMidBeam1 - setCoor.Origin).GetNormal();
                    var directionVectorToMovePointToDesiredLocation1 = new Vector(trMidBeam2 - setCoor.Origin).GetNormal();

                    //insert bolts on the web plate at the specified positions 
                    InsertBolts(webPlate, pick.beam1, setCoor.Origin, new Point((plateLength * 0.5) * directionVectorToMovePointToDesiredLocation), Position.RotationEnum.FRONT);
                    InsertBolts(webPlate, pick.beam2, setCoor.Origin, new Point((plateLength * 0.5) * directionVectorToMovePointToDesiredLocation1), Position.RotationEnum.BACK);

                    InsertBolts(flangePlate.beamTP, pick.beam1, new Point(beamDepth * 0.5 * setCoor.AxisY.GetNormal()), new Point((plateLength * 0.5) * directionVectorToMovePointToDesiredLocation) + beamDepth * 0.5 * setCoor.AxisY.GetNormal(), Position.RotationEnum.BELOW);
                    InsertBolts(flangePlate.beamTP, pick.beam2, new Point(beamDepth * 0.5 * setCoor.AxisY.GetNormal()), new Point((plateLength * 0.5) * directionVectorToMovePointToDesiredLocation1) + beamDepth * 0.5 * setCoor.AxisY.GetNormal(), Position.RotationEnum.TOP);

                    InsertBolts(flangePlate.beamBP, pick.beam1, new Point(-beamDepth * 0.5 * setCoor.AxisY.GetNormal()), new Point((plateLength * 0.5) * directionVectorToMovePointToDesiredLocation - beamDepth * 0.5 * setCoor.AxisY.GetNormal()), Position.RotationEnum.BELOW);
                    InsertBolts(flangePlate.beamBP, pick.beam2, new Point(-beamDepth * 0.5 * setCoor.AxisY.GetNormal()), new Point((plateLength * 0.5) * directionVectorToMovePointToDesiredLocation1 - beamDepth * 0.5 * setCoor.AxisY.GetNormal()), Position.RotationEnum.TOP);
                }
                catch
                {
                    throw new Exception();  //if an exception occurs throw a new generic exception
                }

                finally
                {
                    myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(orginalTP); // restore the original transformation plane
                }
            }
            else
            {
                MessageBox.Show("Invalid option");  //  display msg if the beams are not parallel
            }

        }
        #endregion

        #region GEOMETRIC PLANE
        private GeometricPlane GetGeometricPlane(Beam beam1)
        {
            GeometricPlane geometricPlane = new GeometricPlane(beam1.GetCoordinateSystem());
            return geometricPlane;
        }
        #endregion

        #region PICK 2 BEAM
        private (Beam beam1, Beam beam2) Pick2Beam()
        {
            Beam pickBeam1 = _pick.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "pick the first beam") as Beam;
            Beam pickBeam2 = _pick.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "pick the second beam") as Beam;
            return (pickBeam1, pickBeam2);
        }
        #endregion

        #region SET GLOBAL AND STORE ORIGINAL TRANSFORMATION PLANE
        private TransformationPlane SetGlobalAndStoreOriginal()
        {
            WorkPlaneHandler myWorkPlaneHandler = myModel.GetWorkPlaneHandler();
            TransformationPlane original = myWorkPlaneHandler.GetCurrentTransformationPlane();

            TransformationPlane newTSPlane = new TransformationPlane();

            myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(newTSPlane);

            myModel.CommitChanges();
            return original;
        }
        #endregion

        #region MID POINTS OF PASSING BEAM
        private Point GetMidPoint(Beam beam)
        {
            Point startPoint = beam.StartPoint;
            Point endPoint = beam.EndPoint;
            Point midPoint = new Point((startPoint.X + endPoint.X) * 0.5, (startPoint.Y + endPoint.Y) * 0.5, (startPoint.Z + endPoint.Z) * 0.5);
            return midPoint;
        }
        #endregion

        #region MID POINT BY PASSING TWO POINTS
        private Point GetMidPoint(Point p1, Point p2)
        {
            Point midPoint = new Point((p1.X + p2.X) * 0.5, (p1.Y + p2.Y) * 0.5, (p1.Z + p2.Z) * 0.5);
            return midPoint;
        }
        #endregion

        #region COORDINATE SYSTEM
        public void DrawCoordinateSystem(CoordinateSystem cs, string textAtOrigin)
        {
            double lengthFoot = 2.0;
            GraphicsDrawer graphicsDrawer = new GraphicsDrawer();

            Point X = cs.Origin + cs.AxisX.GetNormal() * lengthFoot * 12 * 25.4;
            Point Y = cs.Origin + cs.AxisY.GetNormal() * lengthFoot * 12 * 25.4;
            Point Z = cs.Origin + cs.AxisX.Cross(cs.AxisY).GetNormal() * lengthFoot * 12 * 25.4;

            graphicsDrawer.DrawText(cs.Origin, textAtOrigin, new Tekla.Structures.Model.UI.Color());
            graphicsDrawer.DrawText(X, "X", new Color( 1, 0, 0));
            graphicsDrawer.DrawText(Y, "Y", new Color( 0, 1, 0));
            graphicsDrawer.DrawText(Z, "Z", new Color( 0, 0, 1));

            graphicsDrawer.DrawLineSegment(new LineSegment(cs.Origin, X), new Color(1, 0, 0));
            graphicsDrawer.DrawLineSegment(new LineSegment(cs.Origin, Y), new Color(0, 1, 0));
            graphicsDrawer.DrawLineSegment(new LineSegment(cs.Origin, Z), new Color(0, 0, 1));
        }
        #endregion

        #region FITTING
        private void InsertFitting(Beam beam, CoordinateSystem cs)
        {
            Fitting fitPlane = new Fitting();
            fitPlane.Plane = new Plane();
            fitPlane.Plane.Origin = new Point();
            fitPlane.Plane.AxisX = cs.AxisX.Cross(cs.AxisY).GetNormal();
            fitPlane.Plane.AxisY = cs.AxisY.GetNormal();
            fitPlane.Father = beam;
            fitPlane.Insert();
            myModel.CommitChanges();
        }
        #endregion

        #region WEB PLATES
        private Beam InsertWebPlate(Beam beam, CoordinateSystem cs)
        {
            double WEB_THICKNESS = 0.0;
            beam.GetReportProperty("WEB_THICKNESS", ref WEB_THICKNESS);
            Point p1 = new Point(plateLength * cs.AxisX.GetNormal() + (WEB_THICKNESS * 0.5) * cs.AxisX.Cross(cs.AxisY).GetNormal());
            Point p2 = new Point(-p1.X, p1.Y, p1.Z);
            string profile = $"PL{plateThickness}*{plateLength}";
            string material = "A36";
            Beam WebPlate = InsertBeam(profile, material, p1, p2, Position.PlaneEnum.MIDDLE, Position.RotationEnum.FRONT, Position.DepthEnum.FRONT);
            myModel.CommitChanges();
            return WebPlate;
        }
        #endregion

        #region FLANGE PLATES
        private (Beam beamTP, Beam beamBP) InsertFlangePlates(Beam beam, CoordinateSystem cs)
        {

            beam.GetReportProperty("HEIGHT", ref beamDepth);

            Point point1TP = new Point(plateLength * cs.AxisX.GetNormal() + (beamDepth * 0.5) * cs.AxisY.GetNormal());
            Point point2TP = new Point(-point1TP.X, point1TP.Y, point1TP.Z);
            string profile = $"PL{plateThickness}*{plateLength}";
            string material = "A36";
            Beam beamTP = InsertBeam(profile, material, point1TP, point2TP, Position.PlaneEnum.RIGHT, Position.RotationEnum.BELOW, Position.DepthEnum.MIDDLE);

            Point point1BP = new Point(plateLength * cs.AxisX.GetNormal() + -(beamDepth * 0.5) * cs.AxisY.GetNormal());
            Point point2BP = new Point(-point1BP.X, point1BP.Y, point1BP.Z);
            string profile1 = $"PL{plateThickness}*{plateLength}";
            string material1 = "A36";
            Beam beamBP = InsertBeam(profile1, material1, point1BP, point2BP, Position.PlaneEnum.LEFT, Position.RotationEnum.BELOW, Position.DepthEnum.MIDDLE);

            myModel.CommitChanges();
            return (beamTP, beamBP);
        }
        #endregion

        #region BEAM
        private Beam InsertBeam(string profile, string material, Point startPoint, Point endPoint, Position.PlaneEnum planeEnum, Position.RotationEnum rotationEnum, Position.DepthEnum depthEnum)
        {
            Beam beam = new Beam(startPoint, endPoint)
            {
                Material = { MaterialString = material },
                Profile = { ProfileString = profile },
                Position =
                {
                    Plane = planeEnum,
                    Rotation = rotationEnum,
                    Depth = depthEnum
                }
            };
            beam.Insert();
            return beam;
        }
        #endregion

        #region BOLT
        private void InsertBolts(Beam webPlate, Beam beam, Point origin, Point point, RotationEnum rotation)
        // you can acess BoltSize ,Tolerance,  BoltStandard in method here by using (double boltsize , double tolarance, string Boltstandard)
        {

            BoltArray MPB = new BoltArray();

            MPB.PartToBeBolted = beam;

            MPB.PartToBoltTo = webPlate;

            MPB.FirstPosition = origin;
            MPB.SecondPosition = point;

            //MPB.BoltSize = 12.7;
            MPB.BoltSize = boltSize;  // this boltsize assign in form1 : form 
            //MPB.Tolerance = 2.00;
            MPB.Tolerance = tolerence;
            //MPB.BoltStandard = "A325N";
            MPB.BoltStandard = boltStandard;
            MPB.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
            //MPB.CutLength = 80;
            MPB.CutLength = cutLength;
            //MPB.Length = 80;
            MPB.Length = Length;
            //MPB.ExtraLength = 0;
            MPB.ExtraLength = extraLength;
            MPB.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_NO;

            //MPB.Position.Depth = Position.DepthEnum.MIDDLE;
            //MPB.Position.Plane = Position.PlaneEnum.LEFT;
            MPB.Position.Rotation = rotation;
            MPB.StartPointOffset.Dx = 2 * 25.4;

            MPB.Bolt = true;
            MPB.Washer1 = true;
            MPB.Washer2 = false;
            MPB.Nut1 = true;
            MPB.Nut2 = false;

            MPB.Hole1 = true;
            MPB.Hole2 = true;
            MPB.Hole3 = true;
            MPB.Hole4 = true;
            MPB.Hole5 = true;

            MPB.AddBoltDistX(50.8);
            //MPB.AddBoltDistX(50.8);
            //MPB.AddBoltDistX(50.8);

            MPB.AddBoltDistY(50.8);
            //MPB.AddBoltDistY(60);
            //MPB.AddBoltDistY(50);

            bool isInsert = MPB.Insert();
            myModel.CommitChanges();
        }
        #endregion


    }
}
