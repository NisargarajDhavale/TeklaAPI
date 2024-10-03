using System;
using System.Collections;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model.UI;
//using Point = Tekla.Structures.Geometry3d;
using static Tekla.Structures.Model.Position;
using Color = Tekla.Structures.Model.UI.Color;
using TSM = Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using System.Diagnostics.Eventing.Reader;
using Tekla.Structures.ModelInternal;

namespace TeklaAPIpracRev
{
    public partial class TeklaOpenAPI : Form
    {
        public TeklaOpenAPI()
        {
            InitializeComponent();
        }
        //  GLOBAL VARIABLES
        #region GLOBAL VARIABLES
        Model myModel = new Model();
        Picker _picker = new Picker();
        private Position.DepthEnum depthEnum;
        private Position.PlaneEnum planeEnum;
        private Position.RotationEnum rotationEnum;
        Beam myColumn = null;
        Beam mybeam = null;
        string colProfile;
        string colMaterial;
        string colClass;
        string beamProfile;
        string beamMaterial;
        string beamClass;
        string platethickness;
        int weldSize;
        const double inch = 25.4;
        double plateLength = 6 * inch;  //  plate length will be 6 * inch >> 6 * 25.4   ==    152.4
        double plateThickness = 0.5 * inch;   //  plate thickness will be 0.5 * inch >> 0.5 * 25.4  ==  12.7
        double beamDepth = 0.0;
        double boltSize = 12.7;
        double tolerance = 2.00;
        string boltStd = "A325N";
        double cutLength = 80;
        double length = 80;
        double extraLength = 0;
        private static ArrayList ObjectListw = new ArrayList(); // weld
        private static ArrayList ObjectList = new ArrayList(); // girt conn
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            if (myModel.GetConnectionStatus())
            {
                MessageBox.Show("Tekla is Connected");
                return;
            }
            else
            {
                MessageBox.Show("Tekla is NOT Connected");
                return;
            }

        }

        //  COLUMN CODE / COLUMN PLANE / COLUMN CMB
        #region COLUMN
        private void btn_Col_Click(object sender, EventArgs e)
        {
            Point startPoint = _picker.PickPoint("pick the start point");
            Point endPoint = new Point(startPoint.X, startPoint.Y, 5000);
            myColumn = new Beam(startPoint, endPoint);
            myColumn.Profile.ProfileString = colProfile;
            myColumn.Material.MaterialString = colMaterial;
            myColumn.Class = Convert.ToString(colClass);
            myColumn.Position.Depth = DepthEnum.MIDDLE; //  Column Position > Horizontal
            myColumn.Position.Plane = planeEnum;        //  Column Position > Vertical
            myColumn.Position.Rotation = rotationEnum;  //  Column Position > Rotation
            myColumn.Insert();

            txt_ColProfile.Clear();
            txt_ColMaterial.Clear();
            txt_ColClass.Clear();
            myModel.CommitChanges();
        }
        private void cmb_ColPlane_SelectedIndexChanged(object sender, EventArgs e)  //  Column Position > Vertical
        {
            int selectedIndexCv = cmb_ColPlane.SelectedIndex;
            planeEnum = updatePlane(selectedIndexCv);
        }
        private Position.PlaneEnum updatePlane(int OnPlane) //  Column Position > Vertical
        {
            switch (OnPlane)
            {
                case 0:
                    return planeEnum = Tekla.Structures.Model.Position.PlaneEnum.LEFT;
                case 1:
                    return planeEnum = Tekla.Structures.Model.Position.PlaneEnum.MIDDLE;
                case 2:
                    return planeEnum = Tekla.Structures.Model.Position.PlaneEnum.RIGHT;
                default:
                    return planeEnum = Tekla.Structures.Model.Position.PlaneEnum.MIDDLE;
            }
        }
        private Position.RotationEnum updateRotation(int OnRotation)    //  Column Position > Rotation
        {
            switch(OnRotation)
            {
                case 0:
                    return rotationEnum = Tekla.Structures.Model.Position.RotationEnum.FRONT;
                case 1:
                    return rotationEnum = Tekla.Structures.Model.Position.RotationEnum.TOP;
                case 2:
                    return rotationEnum = Tekla.Structures.Model.Position.RotationEnum.BACK;
                case 3:
                    return rotationEnum = Tekla.Structures.Model.Position.RotationEnum.BELOW;
                default:
                    return rotationEnum = Tekla.Structures.Model.Position.RotationEnum.TOP;
            }
        }
        private void cmb_ColRotation_SelectedIndexChanged(object sender, EventArgs e)   //  Column Position > Rotation
        {
            int selectIndexCr = cmb_ColRotation.SelectedIndex;
            rotationEnum = updateRotation(selectIndexCr);
        }
        private void txt_ColProfile_TextChanged(object sender, EventArgs e)
        {
            colProfile = txt_ColProfile.Text;
        }
        private void txt_ColMaterial_TextChanged(object sender, EventArgs e)
        {
            colMaterial = txt_ColMaterial.Text;
        }
        private void txt_ColClass_TextChanged(object sender, EventArgs e)
        {
            colClass = txt_ColClass.Text;
        }
        #endregion

        //  BEAM CODE / BEAM PLANE / BEAM CMB
        #region BEAM
        private void btn_Beam_Click(object sender, EventArgs e)
        {
            Point startPoint = _picker.PickPoint("pick the start point of beam");
            Point endPoint = _picker.PickPoint("pick the endPoint of the beam");
            mybeam = new Beam(startPoint, endPoint);
            mybeam.Profile.ProfileString = beamProfile;
            mybeam.Material.MaterialString = beamMaterial;
            mybeam.Class = Convert.ToString(beamClass);
            mybeam.Position.Depth = depthEnum;
            mybeam.Position.Rotation = rotationEnum;
            mybeam.Insert();

            txt_BeamProfile.Clear();
            txt_BeamMaterial.Clear();
            txt_BeamClass.Clear();
            myModel.CommitChanges();
        }
        private void cmb_BeamPlane_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndexB = cmb_BeamPlane.SelectedIndex;
            depthEnum = updateDepthB(selectedIndexB);
        }
        private Position.DepthEnum updateDepthB(int depth)
        {
            switch (depth)
            {
                case 0:
                    return depthEnum = Position.DepthEnum.FRONT;
                case 1:
                    return depthEnum = Position.DepthEnum.MIDDLE;
                case 2:
                    return depthEnum = Position.DepthEnum.BEHIND;
                default:
                    return depthEnum = Position.DepthEnum.MIDDLE;
            }
        }
        private Position.RotationEnum updateRotation1(int OnRotation)
        {
            switch (OnRotation)
            {
                case 0:
                    return rotationEnum = Tekla.Structures.Model.Position.RotationEnum.FRONT;
                case 1:
                    return rotationEnum = Tekla.Structures.Model.Position.RotationEnum.TOP;
                case 2:
                    return rotationEnum = Tekla.Structures.Model.Position.RotationEnum.BACK;
                case 3:
                    return rotationEnum = Tekla.Structures.Model.Position.RotationEnum.BELOW;
                default:
                    return rotationEnum = Tekla.Structures.Model.Position.RotationEnum.TOP;
            }
        }
        private void cmb_BeamRotation_SelectedIndexChanged(object sender, EventArgs e)//  Beam Position > Rotation
        {
            int selectIndexBr = cmb_ColRotation.SelectedIndex;
            rotationEnum = updateRotation1(selectIndexBr);
        }
        private void txt_BeamProfile_TextChanged(object sender, EventArgs e)
        {
            beamProfile = txt_BeamProfile.Text;
        }
        private void txt_BeamMaterial_TextChanged(object sender, EventArgs e)
        {
            beamMaterial = txt_BeamMaterial.Text;
        }
        private void txt_BeamClass_TextChanged(object sender, EventArgs e)
        {
            beamClass = txt_BeamClass.Text;
        }
        #endregion

        //  CONTOUR PLATE CODE
        #region CONTOUR PLATE
        private void btn_ContourPlate_Click(object sender, EventArgs e) //  CONTOUR PLATE
        {
            ArrayList pickedPoints = _picker.PickPoints(Picker.PickPointEnum.PICK_POLYGON, "Pick Polygon Points");
            ArrayList contourPoints = new ArrayList();
            foreach (Point point in pickedPoints)
            {
                ContourPoint contourPoint = new ContourPoint(point, new Chamfer());
                contourPoints.Add(contourPoint);
            }
            ContourPlate CP = new ContourPlate();
            CP.Contour.ContourPoints = contourPoints;
            CP.Profile.ProfileString = platethickness;
            CP.Material.MaterialString = "1500";
            CP.Class = "99";
            CP.Position.Depth = DepthEnum.BEHIND;
            CP.Insert();
            txt_CP.Clear();

            myModel.CommitChanges();
        }
        private void txt_CP_TextChanged(object sender, EventArgs e) //  CONTOUR PLATE
        {
            platethickness = txt_CP.Text;
        }
        #endregion

        //  WELDING CODE
        #region WELDING
        private void btn_Weld_Click(object sender, EventArgs e) //  WELD
        {
            ModelObject primaryPart = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART,"Pick a part");
            ModelObject secondaryPart = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART,"Pick part to be welded");
            ObjectListw.Add(primaryPart);
            ObjectListw.Add(secondaryPart);

            Weld weld = new Weld();
            weld.MainObject = primaryPart;
            weld.SecondaryObject = secondaryPart;
            weld.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld.SizeAbove = weldSize;
            weld.SizeBelow = weldSize;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
            weld.AroundWeld = false;
            weld.ShopWeld = false;
            if (!weld.Insert())
            {
                MessageBox.Show("Weld insert failed");
            }
            txt_WeldSize.Clear();
            myModel.CommitChanges();
        }
        private void txt_WeldSize_TextChanged(object sender, EventArgs e)   //  weld size
        {
            weldSize = Convert.ToInt32(txt_WeldSize.Text);
        }
        #endregion

        //  PARALLEL BEAM  Splice_CONNECTION CODE
        #region SPLICE CONNECTION
        private void btn_SpliceConnParaBeams_Click(object sender, EventArgs e)
        {
            TransformationPlane orginalTP = SetGlobalAndStoreOriginal();    //saving current TransformationPlane and setting new    // variable 'originalTP' of type 'TransformationPlane'

            CoordinateSystem cso = new CoordinateSystem();      //  creates a new coordinate system
            (Beam beam1, Beam beam2) pick = Pick2Beams();       //  pick two beams from the model and returning them as a Tuple

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
        private GeometricPlane GetGeometricPlane(Beam b1)   //  geometric plane
        {
            GeometricPlane geoPlane = new GeometricPlane(b1.GetCoordinateSystem());
            return geoPlane;
        }
        private (Beam b1, Beam b2) Pick2Beams() //  pick 2 beams as Tuple
        {
            Beam pick1Beam = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT,"pick first beam") as Beam;
            Beam pick2Beam = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT,"pick second beam") as Beam;
            return (pick1Beam, pick2Beam);
        }
        private TransformationPlane SetGlobalAndStoreOriginal() //  Transformation Plane
        {
            WorkPlaneHandler myWorkPlaneHandler = myModel.GetWorkPlaneHandler();
            TransformationPlane original = myWorkPlaneHandler.GetCurrentTransformationPlane();
            TransformationPlane newTSPlane = new TransformationPlane();
            myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(newTSPlane);
            myModel.CommitChanges();
            return original;
        }
        private Point GetMidPoint(Beam b)   //  get Mid Point
        {
            Point startPoint = b.StartPoint;
            Point endPoint = b.EndPoint;
            Point midPoint = new Point((startPoint.X + endPoint.X) * 0.5, (startPoint.Y + endPoint.Y) * 0.5, (startPoint.Z + endPoint.Z) * 0.5);
            return midPoint;
        }
        private Point GetMidPoint(Point p1, Point p2) //  midpoint by passsing two points
        {
            Point midPoint = new Point((p1.X + p2.X) * 0.5, (p1.Y + p2.Y) * 0.5, (p1.Z + p2.Z) * 0.5);
            return midPoint;
        }
        public void DrawCoordinateSystem(CoordinateSystem cs, string textAtOrigin)  //  Coordinate System
        {
            double lengthFoot = 2.0;
            GraphicsDrawer graphicDrawer = new GraphicsDrawer();

            Point X = cs.Origin + cs.AxisX.GetNormal() * lengthFoot * 12 * 25.4;
            Point Y = cs.Origin + cs.AxisY.GetNormal() * lengthFoot * 12 * 25.4;
            Point Z = cs.Origin + cs.AxisX.Cross(cs.AxisY).GetNormal() * lengthFoot * 12 * 25.4;

            graphicDrawer.DrawText(cs.Origin, textAtOrigin, new Tekla.Structures.Model.UI.Color());
            graphicDrawer.DrawText(X,"X", new Color(1, 0, 0));
            graphicDrawer.DrawText(Y,"Y", new Color(0, 1, 0));
            graphicDrawer.DrawText(Z,"Z", new Color(0, 0, 1));

            graphicDrawer.DrawLineSegment(new LineSegment(cs.Origin, X), new Color(1,0,0));
            graphicDrawer.DrawLineSegment(new LineSegment(cs.Origin, Y), new Color(0,1,0));
            graphicDrawer.DrawLineSegment(new LineSegment(cs.Origin, Z), new Color(0,0,1));
        }
        private void InsertFitting(Beam b, CoordinateSystem cs) //  Fitting
        {
            Fitting fitplane = new Fitting();
            fitplane.Plane = new Plane();
            fitplane.Plane.Origin = new Point();
            fitplane.Plane.AxisX = cs.AxisX.Cross(cs.AxisY).GetNormal();
            fitplane.Plane.AxisY = cs.AxisY.GetNormal();
            fitplane.Father = b;
            fitplane.Insert();
            myModel.CommitChanges();
        }
        private Beam InsertWebPlate(Beam b, CoordinateSystem cs)    //  Web plates
        {
            double WEB_THICKNESS = 0.0;
            b.GetReportProperty("WEB_THICKNESS", ref WEB_THICKNESS);
            Point p1 = new Point(plateLength * cs.AxisX.GetNormal() + (WEB_THICKNESS * 0.5) * cs.AxisX.Cross(cs.AxisY).GetNormal());
            Point p2 = new Point();
            string profile = $"PL{plateThickness}*{plateLength}";
            string material = "A36";
            Beam webPlate = InsertBeam(profile, material, p1, p2, Position.PlaneEnum.MIDDLE, Position.RotationEnum.FRONT, Position.DepthEnum.FRONT);
            myModel.CommitChanges();
            return webPlate;
        }
        private (Beam beamTP, Beam beamBP) InsertFlangePlates(Beam b, CoordinateSystem cs)  //  Flange Plates
        {
            b.GetReportProperty("HEIGHT", ref beamDepth);
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
        private Beam InsertBeam(string profile, string material, Point startPoint, Point endPoint, Position.PlaneEnum planeEnum, Position.RotationEnum rotationEnum, Position.DepthEnum depthEnum)
        {
            Beam Plate = new Beam(startPoint, endPoint)
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
            Plate.Insert();
            return Plate;
        }
        private void InsertBolts(Beam webPlate, Beam beam, Point origin, Point point, RotationEnum rotation)
        // you can acess BoltSize ,Tolerance,  BoltStandard in method here by using (double boltsize , double tolarance, string Boltstandard)
        {
            BoltArray MPB = new BoltArray();

            MPB.PartToBeBolted = beam;
            MPB.PartToBoltTo = webPlate;

            MPB.FirstPosition = origin;
            MPB.SecondPosition = point;

            MPB.BoltSize = boltSize;  // this boltsize assign in form1 : form 
            MPB.Tolerance = tolerance;
            MPB.BoltStandard = boltStd;
            MPB.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
            MPB.CutLength = cutLength;
            MPB.Length = length;
            MPB.ExtraLength = extraLength;
            MPB.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_NO;

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
            MPB.AddBoltDistY(50.8);

            bool isInsert = MPB.Insert();
            myModel.CommitChanges();
        }
        #endregion

        //  BOLT ARRAY
        #region Bolt Array
        private void btn_BoltArray_Click(object sender, EventArgs e)
        {
            ModelObject modObj1 = _picker.PickObject(TSM.UI.Picker.PickObjectEnum.PICK_ONE_PART);
            Part cP1 = modObj1 as Part;
            ModelObject modObj2 = _picker.PickObject(TSM.UI.Picker.PickObjectEnum.PICK_ONE_PART);
            Part cP2 = modObj2 as Part; // contourplate

            TSM.BoltArray bArr = new TSM.BoltArray();   // bolt array
            bArr.Delete();
            bArr.PartToBeBolted = cP2;
            bArr.PartToBoltTo = cP1;

            //            B.FirstPosition = new TS.Point(44.45, 177.8, 9.525);
            //            B.SecondPosition = new TS.Point(-44.45, 177.8, 9.525);
            bArr.FirstPosition = _picker.PickPoint("Pick First Point");
            bArr.SecondPosition = _picker.PickPoint("Pick Second Point");
            bArr.BoltSize = 25.05;
            bArr.Tolerance = 2.00;
            bArr.BoltStandard = "A307";
            bArr.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
            bArr.Length = 80;
            bArr.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;
            bArr.Position.Depth = Position.DepthEnum.MIDDLE;
            bArr.Position.Plane = Position.PlaneEnum.MIDDLE;
            bArr.Position.Rotation = Position.RotationEnum.FRONT;

            bArr.AddBoltDistX(88.9);
            bArr.StartPointOffset.Dx = 44.45;
            bArr.AddBoltDistY(88.9);

            if (!bArr.Insert())
            {
                MessageBox.Show("BoltArray Insert Failed");
            }
            myModel.CommitChanges();
        }
        #endregion

        //  FITTING
        #region FITTING
        private void btn_Fitting_Click(object sender, EventArgs e)
        {
            createFitting(); 
        }
        private void createFitting()
        {

            ArrayList pickPoints = _picker.PickPoints(Picker.PickPointEnum.PICK_POLYGON, "Pick Polygon Points for fitting");
            ArrayList fittingPoints = new ArrayList();
            foreach (Point point in pickPoints)
            {
                Point fitPoint = new Point();
                fittingPoints.Add(fitPoint);
            }

            Fitting fit = new Fitting();
            fit.Plane = new Plane();
            fit.Father = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick object to apply fitting to: ");
            fit.Plane.Origin = _picker.PickPoint("Pick first point");
            fit.Plane.AxisX = new Vector(_picker.PickPoint("Pick second point"));
            fit.Plane.AxisY = new Vector(_picker.PickPoint("Pick third point"));
            fit.Insert();
            if (!fit.Insert())
            {
                MessageBox.Show("Fitting insertion failed");
            }
        }
        #endregion

        //  GLOBAL COORDINATE SYSTEM
        #region GLOBAL COORDINATE SYSTEM
        private void CreateCoordinateSystem()
        {
            try
            {
                Tekla.Structures.Geometry3d.Point csOrigin = _picker.PickPoint("select a point of origin");
                if (csOrigin == null)
                {
                    MessageBox.Show("no point selected; could not create coordinate system");
                    return;
                }
                Vector xAxis = new Vector(1, 0, 0);
                Vector yAxis = new Vector(0, 1, 0);
                Vector zAxis = xAxis.Cross(yAxis);

                CoordinateSystem cs = new CoordinateSystem(csOrigin, xAxis, yAxis);
                var workPlaneHandler = new Model().GetWorkPlaneHandler();
                var newWorkPlane = new TransformationPlane(cs);

                if (!workPlaneHandler.SetCurrentTransformationPlane(newWorkPlane))
                {
                    MessageBox.Show("Coordinate system failed to insert");
                }
                else
                {
                    myModel.CommitChanges();
                    MessageBox.Show("Coordinate system inserted successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred");
            }

        }
        private void btn_CS_Click(object sender, EventArgs e)
        {
            CreateCoordinateSystem();
        }
        #endregion

        //  GIRT CONNECTION
        #region Girt Connection
        private void btn_GirtConnection_Click(object sender, EventArgs e)
        {
            #region Pick Parts
            ModelObject col = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick primary part");
            ModelObject girt = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick secondary part");
            #endregion
            #region Coordinate System & WorkPlane
            var girtPart = girt as Beam;
            var c = col as Beam;
            Point origin1 = girtPart.StartPoint;
            if (Distance.PointToPoint(girtPart.StartPoint, c.EndPoint) < Distance.PointToPoint(girtPart.EndPoint, c.EndPoint))
            {
                origin1 = girtPart.StartPoint;
                try
                {
                    if (origin1 == null)
                    {
                        MessageBox.Show("No point selected. Coordinate System creation aborted");
                        return;
                    }
                    var girtCoord = (girt as Part).GetCoordinateSystem();
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
            }
            else if (Distance.PointToPoint(girtPart.StartPoint, c.EndPoint) > Distance.PointToPoint(girtPart.EndPoint, c.EndPoint))
            {
                origin1 = girtPart.EndPoint;
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
            }
            //if (Distance.PointToPoint(girtPart.StartPoint, c.EndPoint) > Distance.PointToPoint(girtPart.EndPoint, c.EndPoint)) origin1 = girtPart.EndPoint;

            #endregion
            #region Cut Part (Girt)
            CutPlane cutPlane = new CutPlane();
            Beam girtPart1 = girt as Beam;
            cutPlane.Plane = new Plane();
            cutPlane.Plane.Origin = new Point(63.5, 0, 0);
            cutPlane.Plane.AxisX = new Vector(0, 63.5, 0);
            cutPlane.Plane.AxisY = new Vector(0, 0, -100);
            cutPlane.Father = girtPart1;
            cutPlane.Insert();
            #endregion
            #region Plate (Below Girt)
            ContourPlate cp = new ContourPlate();
            //Point beamSrtPt = beamPart.StartPoint;
            cp.AddContourPoint(new ContourPoint(new Point(0, 50.8, -24.67), null));
            cp.AddContourPoint(new ContourPoint(new Point(127, 50.8, -24.67), null));
            cp.AddContourPoint(new ContourPoint(new Point(127, -50.8, -24.67), null));
            cp.AddContourPoint(new ContourPoint(new Point(0, -50.8, -24.67), null));
            cp.Profile.ProfileString = plateThickness.ToString();
            cp.Material.MaterialString = "43A";
            cp.Class = "1";
            cp.Position.Depth = DepthEnum.BEHIND;
            cp.Insert();
            #endregion
            #region Bolt Array At Plate & Girt
            //Part primaryCol = col as Part;
            Beam girt_Part = girt as Beam;
            Point girtStartPoint = girt_Part.StartPoint;
            Part secondaryPart = cp;
            BoltArray bArr = new BoltArray();
            bArr.Delete();
            bArr.PartToBoltTo = girt_Part;
            bArr.PartToBeBolted = secondaryPart;
            bArr.FirstPosition = girtStartPoint;
            bArr.SecondPosition = girt_Part.EndPoint;
            bArr.BoltSize = 12.7;
            bArr.Tolerance = 2.00;
            bArr.BoltStandard = "A325N";
            bArr.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
            bArr.Length = 80;
            bArr.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;
            bArr.Position.Depth = Position.DepthEnum.MIDDLE;
            bArr.Position.Plane = Position.PlaneEnum.MIDDLE;
            bArr.Position.Rotation = Position.RotationEnum.FRONT;

            bArr.AddBoltDistX(0);
            bArr.StartPointOffset.Dx = 88.9;
            bArr.AddBoltDistY(50.8);
            //bArr.Insert();
            if (!bArr.Insert())
            {
                MessageBox.Show("BoltArray Insert Failed");
            }
            #endregion
            #region Weld (At Plate & Column)
            var colPart = col as Beam;
            Weld weld = new Weld();
            ObjectList.Add(colPart);
            ObjectList.Add(cp);
            weld.Delete();
            weld.MainObject = colPart;
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
            myModel.CommitChanges();
        }
        #endregion

        //  PERPENDICULAR BEAM CONNECTION
        #region Perpendicular Beam Connection
        private void btn_PerpBeamConn_Click(object sender, EventArgs e)
        {
            #region Pick Parts
            ModelObject col = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick primary part");
            ModelObject girt = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick secondary part");
            #endregion
            #region Coordinate System & WorkPlane
            var girtPart = girt as Beam;
            Point origin1 = girtPart.StartPoint;

            try
            {
                if (origin1 == null)
                {
                    MessageBox.Show("No point selected. Coordinate System creation aborted");
                    return;
                }
                var girtCoord = (girt as Part).GetCoordinateSystem();
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
            #endregion
            #region Cut Part (Girt)
            CutPlane cutPlane = new CutPlane();
            Beam girtPart1 = girt as Beam;
            cutPlane.Plane = new Plane();
            cutPlane.Plane.Origin = new Point(63.5, 0, 0);
            cutPlane.Plane.AxisX = new Vector(0, 63.5, 0);
            cutPlane.Plane.AxisY = new Vector(0, 0, -100);
            cutPlane.Father = girtPart1;
            cutPlane.Insert();
            #endregion
            #region Plate (Below Girt)
            ContourPlate cp = new ContourPlate();
            //Point beamSrtPt = beamPart.StartPoint;
            cp.AddContourPoint(new ContourPoint(new Point(0, 50.8, -2.35), null));
            cp.AddContourPoint(new ContourPoint(new Point(127, 50.8, -2.35), null));
            cp.AddContourPoint(new ContourPoint(new Point(127, -50.8, -2.35), null));
            cp.AddContourPoint(new ContourPoint(new Point(0, -50.8, -2.35), null));
            cp.Profile.ProfileString = plateThickness.ToString();
            cp.Material.MaterialString = "43A";
            cp.Class = "1";
            cp.Position.Depth = DepthEnum.BEHIND;
            cp.Insert();
            #endregion
            #region Bolt Array At Plate & Girt
            //Part primaryCol = col as Part;
            Beam girt_Part = girt as Beam;
            Point girtStartPoint = girt_Part.StartPoint;
            Part secondaryPart = cp;
            BoltArray bArr = new BoltArray();
            bArr.Delete();
            bArr.PartToBoltTo = girt_Part;
            bArr.PartToBeBolted = secondaryPart;
            bArr.FirstPosition = girtStartPoint;
            bArr.SecondPosition = girt_Part.EndPoint;
            bArr.BoltSize = 12.7;
            bArr.Tolerance = 2.00;
            bArr.BoltStandard = "A325N";
            bArr.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
            bArr.Length = 80;
            bArr.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;
            bArr.Position.Depth = Position.DepthEnum.MIDDLE;
            bArr.Position.Plane = Position.PlaneEnum.MIDDLE;
            bArr.Position.Rotation = Position.RotationEnum.FRONT;

            bArr.AddBoltDistX(0);
            bArr.StartPointOffset.Dx = 88.9;
            bArr.AddBoltDistY(50.8);
            bArr.Insert();
            {
                MessageBox.Show("BoltArray Insert Failed");
            }
            #endregion
            #region Weld (At Plate & Column)
            var colPart = col as Beam;
            Weld weld = new Weld();
            ObjectListw.Add(colPart);
            ObjectListw.Add(cp);
            weld.Delete();
            weld.MainObject = colPart;
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
            myModel.CommitChanges();
        }
        #endregion


    }
}