﻿'+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
'* Copyright (C) 2013-2015 NamCore Studio <https://github.com/megasus/Namcore-Studio>
'*
'* This program is free software; you can redistribute it and/or modify it
'* under the terms of the GNU General Public License as published by the
'* Free Software Foundation; either version 3 of the License, or (at your
'* option) any later version.
'*
'* This program is distributed in the hope that it will be useful, but WITHOUT
'* ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
'* FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
'* more details.
'*
'* You should have received a copy of the GNU General Public License along
'* with this program. If not, see <http://www.gnu.org/licenses/>.
'*
'* Developed by Alcanmage/megasus
'*
'* //FileInfo//
'*      /Filename:      DevExtension
'*      /Description:   Extension for development purposes
'+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
Imports System.Runtime.CompilerServices
Imports System.IO
Imports NCFramework.Framework.Logging
Imports NCFramework.Framework.Modules
Imports System.Xml.Serialization
Imports System.Xml

Namespace Modules.Interface
    Public Module DevExtension
        <Extension()>
        Public Sub CheckTag(ByVal ctrl As Control)
            If TypeOf (ctrl.Tag) Is Item Then
                Dim locItm As Item = CType(ctrl.Tag, Item)
                Dim x As New XmlSerializer(locItm.GetType)
                Dim memStream As New MemoryStream
                Dim sw As New StreamWriter(memStream)
                x.Serialize(sw, locItm)
                memStream.Position = 0
                Dim sr As New StreamReader(memStream)
                Dim serializedXml As New XmlDocument
                serializedXml.Load(sr)
                LogAppend(serializedXml.OuterXml, "DevExtension_CheckTag", False, False)
            End If
        End Sub
    End Module
End Namespace