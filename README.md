# **ScreenStealer c# App**

This application capture the screen or active windows, and save them to files, or sends as Base64 images.

[GitHub](https://github.com/RedAty/screen-stealer)

___

Base functions:

   * Base64 output to stdout
   * HTML <img> output 
   * Image file output: jpeg,png,bmp,gif,tiff
    
## Usage

```
ScreenStealer.exe <window/screen> <type> <options/timer>
```

#####Base64 output to stdout
```
ScreenStealer.exe console
ScreenStealer.exe window console
ScreenStealer.exe screen console
```

#####HTML Output to <img>
```
ScreenStealer.exe window filename.html append
ScreenStealer.exe window filename.html
ScreenStealer.exe screen filename.html append
ScreenStealer.exe screen filename.html
```

#####Image file output
If you provide path without extension and an integer after that,
the application will create a series of images for that time period what you defined.

Example for 10 seconds of images about screen and window:
```
ScreenStealer.exe window folder/ 10   //10 seconds images
ScreenStealer.exe screen folder/ 10
```
Example about basic file output with multiple extensions:
```
ScreenStealer.exe screen imagefile.png
ScreenStealer.exe window imagefile.gif
ScreenStealer.exe window imagefile.bmp
```

## References
For my work i used StackOverFlow and the following places:

   * https://gallery.technet.microsoft.com/scriptcenter/eeff544a-f690-4f6b-a586-11eea6fc5eb8
   * https://www.hanselman.com/blog/HowDoYouUseSystemDrawingInNETCore.aspx

## Licence
GNU General Public License v3.0 Attila Reterics

I made this project for educational purpose (practicing), free to use.
When you edit please attach References also.

