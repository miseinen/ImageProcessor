using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Globalization;

namespace ImageProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            PhotoFolder folder = new PhotoFolder();
            Console.WriteLine("Input path:");
            PhotoFolder.Path = Console.ReadLine();
            //folder.CopyPhoto();
            folder.PhotoMark();
            Console.ReadLine();
        }
    }
    public class PhotoFolder
    {
        public static string Path { get; set; }
        public static string NewPath { get; set; }
        
        public void CopyPhoto()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(Path);
            if (Directory.Exists(Path))//проверка существует ли путь
            {
                var name = new DirectoryInfo(Path).Name;
                var NewPathName = $"{name}_DateMark";//имя новой папки
                var NewPath = $"{Directory.GetParent(Path)}" +$"{NewPathName}";//путь к новой папке
                Directory.CreateDirectory(NewPath);//создание новой папки
                FileInfo fileInfo = new FileInfo(Path);
                foreach (var image in Directory.EnumerateFiles(Path, ".", SearchOption.AllDirectories).
                    Where(i => i.EndsWith(".jpg") || i.EndsWith(".jpeg") || i.EndsWith(".png") || i.EndsWith(".gif") ||
                    i.EndsWith(".bmp") || i.EndsWith(".svg")))//сортировка "только файлы изображений"
                {
                    var imageName=System.IO.Path.GetFileName(image);//имя с расширением
                    string dateTaken;//переменная для хранения значения даты создания в строке
                    Image im = Image.FromFile(image);
                    try
                    {
                        var property = im.GetPropertyItem(0x0132).Value;//получение метаданных о дате съемки
                        var date = Encoding.UTF8.GetString(property);
                        dateTaken=date.Replace(":", "-");//замена запрещенных для пути символов
                        //устранение скрытых символов
                        dateTaken = new string(dateTaken.Where(c => !char.IsControl(c)).ToArray());
                    }
                    catch (ArgumentException)//если метаданные отстутсвуют
                    {
                        var dateCreation = File.GetCreationTime(image);//получение даты создания
                        dateTaken = dateCreation.ToString("yyyy-dd-MM hh-mm-ss");//приведение к единому стилю
                        dateTaken = new string(dateTaken.Where(c => !char.IsControl(c)).ToArray());
                    }
                    var imageNewName =($"{dateTaken}_{imageName}");//новое имя файла с учетом даты съемки
                    var destFile = System.IO.Path.Combine(NewPath, imageNewName);
                    File.Copy(image, destFile, true);//копирование переименованных файлов в новую папку
                }
            }
            else Console.WriteLine("Folder do not exist");
        }
        public void PhotoMark()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(Path);
            if (Directory.Exists(Path))
            {
                var name = new DirectoryInfo(Path).Name;
                var NewPathName = $"{name}_PhotoMark";//имя новой папки
                var NewPath = $"{Directory.GetParent(Path)}" + $"{NewPathName}";//путь к новой папке
                Directory.CreateDirectory(NewPath);//создание новой папки
                FileInfo fileNewInfo = new FileInfo(NewPath);
                foreach (var image in Directory.EnumerateFiles(Path, ".", SearchOption.AllDirectories).
                    Where(i => i.EndsWith(".jpg") || i.EndsWith(".jpeg") || i.EndsWith(".png") || i.EndsWith(".gif") ||
                    i.EndsWith(".bmp") || i.EndsWith(".svg")))//сортировка "только файлы изображений"
                {
                    var imageName = System.IO.Path.GetFileName(image);//имя с расширением
                    string dateTaken;//переменная для хранения значения даты создания в строке
                    Image im = Image.FromFile(image);
                    try
                    {
                        var property = im.GetPropertyItem(0x0132).Value;//получение метаданных о дате съемки
                        var date = Encoding.UTF8.GetString(property);
                        dateTaken = date.Replace(":", "-");//замена запрещенных для пути символов
                        //устранение скрытых символов
                        dateTaken = new string(dateTaken.Where(c => !char.IsControl(c)).ToArray());
                    }
                    catch (ArgumentException)//если метаданные отстутсвуют
                    {
                        var dateCreation = File.GetCreationTime(image);//получение даты создания
                        dateTaken = dateCreation.ToString("yyyy-dd-MM hh-mm-ss");//приведение к единому стилю
                        dateTaken = new string(dateTaken.Where(c => !char.IsControl(c)).ToArray());
                    }
                    Image imageText = Bitmap.FromFile(image);
                    Graphics graphics = Graphics.FromImage(imageText);
                    Rectangle rect = new Rectangle();
                    graphics.DrawString(dateTaken, new Font("Segoe UI", 25),
                    new SolidBrush(Color.Red), rect.Right, rect.Top);
                    graphics.Flush();//маркировка фото датой
                    var imageMarkedName = "Marked" + imageName;//установление нового имени
                    imageText.Save(System.IO.Path.Combine(NewPath, imageMarkedName));
                    imageText.Dispose();
                }
            }
            else Console.WriteLine("Folder do not exist");
        }
    }
}
