using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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
            Console.WriteLine("Welcome to ImageProcessor\n" +
                "You can Copy your photo, Mark by creation date or Sort by year\n" +
                "CommandBar: copy, mark,create");
            while (true)
            {
                Console.WriteLine("Input path:");
                PhotoFolder.Path = Console.ReadLine();
                DirectoryInfo dirInfo = new DirectoryInfo(PhotoFolder.Path);
                if (Directory.Exists(PhotoFolder.Path))//проверка существует ли путь
                {
                    command:
                    Console.WriteLine("Input command:");
                    var command = Console.ReadLine();
                    
                    switch (command)
                    {
                        case "copy":
                            PhotoFolder.NewPath = folder.CreateFolder(command);
                            folder.CopyPhoto();
                            Console.WriteLine("Complete");
                            break;
                        case "mark":
                            PhotoFolder.NewPath = folder.CreateFolder(command);
                            folder.MarkPhoto();
                            Console.WriteLine("Complete");
                            break;
                        case "sort":
                            PhotoFolder.NewPath = folder.CreateFolder(command);
                            folder.SortByYear();
                            Console.WriteLine("Complete");
                            break;
                        default:
                            Console.WriteLine("Incorrect command");
                            goto command;
                    }
                }
                else Console.WriteLine("Folder do not exist");
            }
        }
    }
    public class PhotoFolder
    {
        public static string Path { get; set; }
        public static string NewPath { get; set; }
        public string CreateFolder(string command)
        {
            var name = new DirectoryInfo(Path).Name;
            var NewPathName = $"{name}_{command.ToUpper()}";//имя новой папки
            NewPath = $"{Directory.GetParent(Path)}" + $"{NewPathName}";//путь к новой папке
            Directory.CreateDirectory(NewPath);//создание новой папки
            return NewPath;
        }
        public string GetDate(string  image)
        {
            string dateTaken;
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
                //получение даты изменения файла 
                //если брать дату cоздания, то при копировании она будет менять на текущую
                var dateCreation = File.GetLastWriteTime(image);
                dateTaken = dateCreation.ToString("yyyy-dd-MM hh-mm-ss");//приведение к единому стилю
                dateTaken = new string(dateTaken.Where(c => !char.IsControl(c)).ToArray());
            }
            return dateTaken;
        }

        public void CopyPhoto()//Переименование изображении в соответствии с датой сьемки
        {
            var thread = new Thread(() =>
              {
                  foreach (var image in Directory.EnumerateFiles(Path, ".", SearchOption.AllDirectories).
                    Where(i => i.EndsWith(".jpg") || i.EndsWith(".jpeg") || i.EndsWith(".png") || i.EndsWith(".gif") ||
                    i.EndsWith(".bmp") || i.EndsWith(".svg")))//сортировка "только файлы изображений"
                  {
                      string dateTaken;
                      dateTaken = GetDate(image);
                      var imageName = System.IO.Path.GetFileName(image);//имя с расширением
                      var imageNewName = ($"{dateTaken}_{imageName}");//новое имя файла с учетом даты съемки
                      var destFile = System.IO.Path.Combine(NewPath, imageNewName);
                      File.Copy(image, destFile, true);//копирование переименованных файлов в новую папку
                  }
              });
            thread.Start();
            
        }

        public void MarkPhoto()//Добавления на изображение отметку, когда фото было сделано
        {
            var thread = new Thread(() =>
            {
                foreach (var image in Directory.EnumerateFiles(Path, ".", SearchOption.AllDirectories).
                   Where(i => i.EndsWith(".jpg") || i.EndsWith(".jpeg") || i.EndsWith(".png") || i.EndsWith(".gif") ||
                   i.EndsWith(".bmp") || i.EndsWith(".svg")))//сортировка "только файлы изображений"
                {
                    string dateTaken;
                    dateTaken = GetDate(image);
                    var imageName = System.IO.Path.GetFileName(image);//имя с расширением
                    Image imageText = Bitmap.FromFile(image);
                    Graphics graphics = Graphics.FromImage(imageText);
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//сглаживание
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    Rectangle rect = new Rectangle();
                    graphics.DrawString(dateTaken, new Font("Segoe UI", 14),
                    new SolidBrush(Color.Red), rect.Right, rect.Top);//маркировка фото датой
                    graphics.Flush();
                    var imageMarkedName = "Marked_" + imageName;//установление нового имени
                    imageText.Save(System.IO.Path.Combine(NewPath, imageMarkedName));
                    imageText.Dispose();
                }
            });
            thread.Start();
        }
        public void SortByYear()//Сортировка изображений по папкам по годам
        {
            var thread = new Thread(() =>
            {
                foreach (var image in Directory.EnumerateFiles(Path, ".", SearchOption.AllDirectories).
                    Where(i => i.EndsWith(".jpg") || i.EndsWith(".jpeg") || i.EndsWith(".png") || i.EndsWith(".gif") ||
                    i.EndsWith(".bmp") || i.EndsWith(".svg")))//сортировка "только файлы изображений"
                {
                    string dateTaken;
                    dateTaken = GetDate(image);
                    dateTaken = dateTaken.Remove(4);//вычленение года съемки
                    var imageName = System.IO.Path.GetFileName(image);//имя с расширением
                    DirectoryInfo newDirInfo = new DirectoryInfo(NewPath);
                    newDirInfo.CreateSubdirectory(dateTaken);//создание подпапки
                    var sourceFileName = System.IO.Path.Combine(Path, imageName);
                    var YearFileName = System.IO.Path.Combine(NewPath, dateTaken);//создание имени папки с годом
                    var destFileName = System.IO.Path.Combine(YearFileName, imageName);
                    File.Copy(sourceFileName, destFileName);//сортировка файлов по годам
                }
            });
            thread.Start();
        }
    }
}
