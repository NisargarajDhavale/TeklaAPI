using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Solid;
using Tekla.Structures.Model.UI;
using static Tekla.Structures.Model.Position;
using Color = Tekla.Structures.Model.UI.Color;
using Point = Tekla.Structures.Geometry3d.Point;
using System.Security.Policy;
using System.Security.Cryptography;
using RenderData;

namespace GirtConnWFA
{
    public partial class GirtConnection : Form
    {
        #region Global Coordinates
        Model myModel = new Model();
        Picker _pick = new Picker();

        double plateThickness = 10; //  plate

        private static ArrayList ObjectList = new ArrayList(); // weld

        int nFaces = 0, nLoops = 0, nVertexes = 0;

        double beamDepth = 0.0;
        const double inch = 25.4;
        double plateLength = 6 * inch;  //  plate length will be 6 * inch >> 6 * 25.4   ==    152.4
        double plateThk = 0.5 * inch;   //  plate thickness will be 0.5 * inch >> 0.5 * 25.4  ==  12.7

        double boltsize = 12.7;
        double tolarance = 2.00;
        string boltStandard = "A325N";
        double cutLength = 80;
        double length = 80;
        double extraLength = 0;
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

        #region Girt Connection

        #region SupportMethods
        /*        public static Point PointToPlane(Point point, Part col)
                {
                    GeometricPlane plane = new GeometricPlane(col.GetCoordinateSystem());
                    return Projection.PointToPlane(point, plane);
                }
                private Point MidPoint(Point point, Point point1)// Calculates the midpoint between two 3D points.
                {
                    Point mid = new Point((point.X + point1.X) / 2, (point.Y + point1.Y) / 2, (point.Z + point1.Z) / 2);
                    return mid;
                }
        */
        /*        private static GeometricPlane ConvertFaceToGeometricPlane(Face face) //converts a face (a flat surface) into a geometric plane using points from the face.
                {
                    ArrayList points = new ArrayList();
                    // Get the edges from the face (since 'Points' is not available)
                    LoopEnumerator loopEnumerator = face.GetLoopEnumerator();
                    while (loopEnumerator.MoveNext())
                    {

                        Loop loop = loopEnumerator.Current as Loop; //  current loop
                        VertexEnumerator vertexEnumerator = loop.GetVertexEnumerator(); //  Get the vertices (corners) of the loop.
                        while (vertexEnumerator.MoveNext()) // Loop through each vertex and add it to the points list.
                        {
                            points.Add(vertexEnumerator.Current);
                        }
                    }
                    // Retrieve the first three points from the list.
                    Point point1 = points[0] as Point;
                    Point point2 = points[1] as Point;
                    Point point3 = points[2] as Point;

                    // Check if we have enough points to define a plane.
                    if (point1 == null || point2 == null || point3 == null)
                    {
                        throw new ArgumentException("The face does not have sufficient points to define a plane.");
                    }

                    // Create vectors from the points
                    Vector vector1 = new Vector(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
                    Vector vector2 = new Vector(point3.X - point1.X, point3.Y - point1.Y, point3.Z - point1.Z);

                    // Calculate the normal vector (cross product of the two vectors)
                    Vector normalVector = Vector.Cross(vector1, vector2);
                    normalVector.Normalize();

                    // Create the geometric plane using point1 and the normal vector
                    GeometricPlane geometricPlane = new GeometricPlane(point1, normalVector);

                    return geometricPlane;
                }
        */
        /*        private List<Face> get_faces(Part beam) //  retrieves the faces of a 3D part and returns them as a List
                {
                    Solid solid = beam.GetSolid();  //  solid representation of the beam
                    FaceEnumerator faceEnumerator = solid.GetFaceEnumerator();  //  enumerator to go through all the faces of solid
                    List<Face> faces = new List<Face>();    //  List to store all the faces
                    while (faceEnumerator.MoveNext())
                    {
                        Face face = faceEnumerator.Current as Face; // Get the current face from the enumerator and cast it to a Face type. 
                        faces.Add(face);    // Add the current face to our list of faces.
                    }
                    return faces;
                }
        */
        /*        private ArrayList Get_Points(Face face) //  retrieves all points(vertices) from a given face and returns them in an ArrayList
                {
                    ArrayList points = new ArrayList();
                    LoopEnumerator loopEnumerator = face.GetLoopEnumerator();   // Get the loops (closed edges) from the face.
                    while (loopEnumerator.MoveNext())
                    {
                        Loop loop = loopEnumerator.Current as Loop;
                        VertexEnumerator vertexEnumerator = loop.GetVertexEnumerator();// Get the vertices (points) of the loop.
                        while (vertexEnumerator.MoveNext())
                        {
                            points.Add(vertexEnumerator.Current);
                        }
                    }
                    return points;
                }
        */

        private GeometricPlane GetGeometryPlane(Beam beam1) //  GeometricPlane
        {
            GeometricPlane geometricPlane = new GeometricPlane(beam1.GetCoordinateSystem());
            return geometricPlane;
        }
        private (Beam beam1, Beam beam2) Pick2Beams()   //  Pick2Beams
        {
            Beam pickBeam1 = _pick.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick first beam") as Beam;
            Beam pickBeam2 = _pick.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick second beam") as Beam;
            return (pickBeam1, pickBeam2);
        }
        private TransformationPlane SetGlobalAndStoreOriginal() //  SetGlobalAndStoreOriginal
        {
            WorkPlaneHandler myWorkPlaneHandler = myModel.GetWorkPlaneHandler();
            TransformationPlane original = myWorkPlaneHandler.GetCurrentTransformationPlane();

            TransformationPlane newTSPlane = new TransformationPlane();

            myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(newTSPlane);
            myModel.CommitChanges();
            return original;
        }
        private Point GetMidPoint(Beam beam)    //  MidPoint by passing beam
        {
            Point startPoint = beam.StartPoint;
            Point endPoint = beam.EndPoint;
            Point midPoint = new Point((startPoint.X + endPoint.X) * 0.5, (startPoint.Y + endPoint.Y) * 0.5, (startPoint.Z + endPoint.Z) * 0.5);
            return midPoint;
        }
        private Point GetMidPoint(Point point1, Point point2)  //  midPoint by passing 2 point
        {
            Point midPoint = new Point((point1.X + point2.X) * 0.5, (point1.Y + point2.Y) * 0.5, (point1.Z + point2.Z) * 0.5);
            return midPoint;
        }
        public void DrawCoordinateSystem(CoordinateSystem coordinateSystem, string textAtOrigin)    //  draw CoordinateSystem
        {
            double lengthFoot = 2.0;
            Tekla.Structures.Model.UI.GraphicsDrawer graphicsDrawer = new GraphicsDrawer();


            Point X = coordinateSystem.Origin + coordinateSystem.AxisX.GetNormal() * lengthFoot * 12 * 25.4;
            Point Y = coordinateSystem.Origin + coordinateSystem.AxisY.GetNormal() * lengthFoot * 12 * 25.4;
            Point Z = coordinateSystem.Origin + coordinateSystem.AxisX.Cross(coordinateSystem.AxisY).GetNormal() * lengthFoot * 12 * 25.4;


            graphicsDrawer.DrawText(coordinateSystem.Origin, textAtOrigin, new Color());
            graphicsDrawer.DrawText(X, "X", new Color(1, 0, 0));
            graphicsDrawer.DrawText(Y, "Y", new Color(0, 1, 0));
            graphicsDrawer.DrawText(Z, "Z", new Color(0, 0, 1));


            graphicsDrawer.DrawLineSegment(new LineSegment(coordinateSystem.Origin, X), new Color(1, 0, 0));
            graphicsDrawer.DrawLineSegment(new LineSegment(coordinateSystem.Origin, Y), new Color(0, 1, 0));
            graphicsDrawer.DrawLineSegment(new LineSegment(coordinateSystem.Origin, Z), new Color(0, 0, 1));
        }
        private void InsertCut(Beam beam, CoordinateSystem cs) //  insert CUT
        {
            CutPlane cutPlane = new CutPlane();
            Beam beamPart = beam as Beam;
            cutPlane.Plane = new Plane();
            cutPlane.Plane.Origin = new Point();
            cutPlane.Plane.AxisX = new Vector(0, 63.5, 0);
            cutPlane.Plane.AxisY = new Vector(0, 0, -100);
            //cutPlane.Plane.AxisX = cs.AxisX.Cross(cs.AxisY).GetNormal();
            //cutPlane.Plane.AxisY = cs.AxisY.GetNormal();
            cutPlane.Father = beam;
            cutPlane.Insert();
        }
        private void InsertPlate(Beam beam, CoordinateSystem cs)  //  insert Plate
        {
            beam.GetReportProperty("WIDTH", ref beamDepth);

            Point point1BP = new Point(plateLength * cs.AxisX.GetNormal() + -(beamDepth * 0.5) * cs.AxisY.GetNormal());
            Point point2BP = new Point(-point1BP.X, point1BP.Y, point1BP.Z);
            string profile1 = $"PL{plateThk}*{plateLength}";
            string material1 = "A36";
            Beam beamBP = InsertBeam(profile1, material1, point1BP, point2BP, Position.PlaneEnum.LEFT, Position.RotationEnum.BELOW, Position.DepthEnum.MIDDLE);

            myModel.CommitChanges();
            //            return (beamBP);
        }
        private Beam InsertBeam(string profile, string material, Point startPoint, Point endPoint, Position.PlaneEnum planeEnum, Position.RotationEnum rotationEnum, Position.DepthEnum depthEnum)  //  
        {
            Beam beam = new Beam(startPoint, endPoint)
            {
                Material = { MaterialString = material },
                Profile = { ProfileString = profile },
                Position =
                {
                    Plane= planeEnum,
                    Rotation= rotationEnum,
                    Depth= depthEnum

                }
            };
            beam.Insert();
            return beam;
        }
        private void InsertBolts(Beam webPlate, Beam beam, Point origin, Point point, RotationEnum rotation)
        // you can acess BoltSize ,Tolerance,  BoltStandard in method here by using (double boltsize , double tolarance, string Boltstandard)
        {

            BoltArray MPB = new BoltArray();

            MPB.PartToBeBolted = beam;

            MPB.PartToBoltTo = webPlate;

            MPB.FirstPosition = origin;
            MPB.SecondPosition = point;

            MPB.BoltSize = boltsize;  // this boltsize assign in form1 : form 
            MPB.Tolerance = tolarance;
            MPB.BoltStandard = boltStandard;
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
        private void btn_GirtConn_Click(object sender, EventArgs e)
        {
            #region Pick Parts
            ModelObject col = _pick.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick primary part");
            ModelObject girt = _pick.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick secondary part");
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

            /*            TransformationPlane originalTP = SetGlobalAndStoreOriginal();

                        CoordinateSystem cso  = new CoordinateSystem();
                        (Beam col, Beam girt) pick = Pick2Beams();


                        #region Pick 2 Parts
                        ModelObject col = _pick.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick primary part");
                        ModelObject girt = _pick.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick secondary part");
                        #endregion
                        #region CoordinateSystem
                        var girtPart = girt as Beam;
                        Point origin11 = girtPart.StartPoint;


                        try
                        {
                            if(origin11 == null)
                            {
                                MessageBox.Show("No point selected");
                                return;
                            }
                            var girtCoord = (girt as Part).GetCoordinateSystem();
                            var workPlaneHandler = new Model().GetWorkPlaneHandler();
                            var newWorkPlane = new TransformationPlane(girtCoord);

                            var colCoord = (col as Part).GetCoordinateSystem();
                            var colWorkPlane = new TransformationPlane(colCoord);


                            new Model().CommitChanges();
                
                            if(girtCoord == null)
                            {
                                throw new ArgumentNullException(nameof(girtCoord));
                            }
                            var gd = new GraphicsDrawer();
                
                            var origin = new Point(Intersection.LineToPlane((girt as Part).GetCenterLine(false), colWorkPlane(false));




                        }
                        catch(Exception ex)
                        {

                        }
                        #endregion*/
            myModel.CommitChanges();
        }
        #endregion
    }
}