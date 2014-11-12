/*
 	Deriva
	An experimental/contemplative game

	Copyright 2014 Caio Hideki Matsumoto (hmcaio@hotmail.com)

	This file is part of Deriva.

		Deriva is free software: you can redistribute it and/or modify
		it under the terms of the GNU General Public License as published by
		the Free Software Foundation, either version 3 of the License, or
		(at your option) any later version.

		Deriva is distributed in the hope that it will be useful,
		but WITHOUT ANY WARRANTY; without even the implied warranty of
		MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
		GNU General Public License for more details.

		You should have received a copy of the GNU General Public License
		along with Deriva.  If not, see <http://www.gnu.org/licenses/>.
 */


using System;
using System.IO;

public static class FileMan
{
    private static StreamWriter writer;


    public static void Open(string path)
    {
        try
        {
            writer = new StreamWriter(path, false);
            //print("Opened");
            MyDebug.print("Opened");
            writer.WriteLine(System.DateTime.Now.ToString());
        }
        catch (Exception e)
        {
            //print(e.Message);
            MyDebug.print(e.Message);
        }
    }

    public static void Close()
    {
        try
        {
            if (writer != null)
            {
                writer.WriteLine(System.DateTime.Now.ToString());
                writer.Close();
                writer = null;
                //print("Closed");
                MyDebug.print("Closed");
            }
        }
        catch (Exception e)
        {
            //print(e.Message);
            MyDebug.print(e.Message);
        }
    }

    public static void Write(object o)
    {
        try
        {
            //print(o.ToString());
            MyDebug.print(o.ToString());
            writer.WriteLine(o.ToString());
        }
        catch (Exception e)
        {
            //print(e.Message);
            MyDebug.print(e.Message);
        }
    }
}
