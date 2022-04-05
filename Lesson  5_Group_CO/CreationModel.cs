using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lesson__5_Group_CO
{
    [TransactionAttribute(TransactionMode.Manual)]

    public class CreationModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            List<Level> listlevel = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .OfType<Level>()
                .ToList();

            Level level1 = listlevel
               .Where(x => x.Name.Equals("Уровень 1"))
               .FirstOrDefault();
            Level level2 = listlevel
                .Where(x => x.Name.Equals("Уровень 2"))
                .FirstOrDefault();

            GetWallDrawMethod(doc, level1, level2);


            /*var res1 = new FilteredElementCollector(doc)
                    .OfClass(typeof(WallType))
                    .OfType<WallType>()
                    .ToList();

            var res2 = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilyInstance))
                    .OfCategory(BuiltInCategory.OST_Doors)
                    .OfType<FamilyInstance>()
                    .Where(f => f.Name.Equals("0915 x 2134 мм"))
                    .ToList();

            var res3 = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .ToList();*/

            return Result.Succeeded;
        }

        public static void GetWallDrawMethod(Document doc, Level level1, Level level2)
        {
            double width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters);
            double dx = width / 2;
            double dy = depth / 2;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));

            Transaction transaction = new Transaction(doc, "Построение стены");
            transaction.Start();

            List<Wall> walls = new List<Wall>();
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                Wall wall = Wall.Create(doc, line, level1.Id, false);
                walls.Add(wall);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);

            }
            AddDoor(doc, level1, walls[0]);

            List<XYZ> pointcenterlist = new List<XYZ>();
            for (int c = 1; c < 4; c++)
            {
                //XYZ pointcenter = GetElementCenter(walls[c]);
                //pointcenterlist.Add(pointcenter);
                AddWindow(doc, level1, walls[c]);
            }

            transaction.Commit();
        }

        public static void AddWindow(Document doc, Level level1, Wall wall)
        {
            FamilySymbol windowType = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_Windows)
                    .OfType<FamilySymbol>()
                    .Where(x => x.Name.Equals("0406 x 0610 мм"))
                    .Where(x => x.FamilyName.Equals("Фиксированные"))
                    .FirstOrDefault();
            XYZ correct = new XYZ(0, 0, 5);
            //BoundingBoxXYZ bounds = wall.get_BoundingBox(null);
            LocationCurve hostCurve = wall.Location as LocationCurve;
            //XYZ point1 = bounds.Max;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            //XYZ point2 = bounds.Min; 
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ pointw = ((point1 + point2) / 2) + correct;
            //XYZ pointc = (point + pointcenter) / 2;

            if (!windowType.IsActive)
                windowType.Activate();

            doc.Create.NewFamilyInstance(pointw, windowType, wall, level1, StructuralType.NonStructural);

            
        }

        public static void AddDoor(Document doc, Level level1, Wall wall)
        {
            FamilySymbol doorType = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_Doors)
                    .OfType<FamilySymbol>()
                    .Where(x => x.Name.Equals("0762 x 2134 мм"))
                    .Where(x => x.FamilyName.Equals("Одиночные-Щитовые"))
                    .FirstOrDefault();
            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ pointd = (point1 + point2)/2;

            if (!doorType.IsActive)
                doorType.Activate();

            doc.Create.NewFamilyInstance(pointd, doorType, wall, level1, StructuralType.NonStructural);
        }
        /*public static XYZ GetElementCenter(Element element)
        {
            BoundingBoxXYZ bounds = element.get_BoundingBox(null);
            return (bounds.Max+bounds.Min)/2;
        }*/
    }
}
