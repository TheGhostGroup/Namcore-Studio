﻿'+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
'* Copyright (C) 2013 Namcore Studio <https://github.com/megasus/Namcore-Studio>
'*
'* This program is free software; you can redistribute it and/or modify it
'* under the terms of the GNU General Public License as published by the
'* Free Software Foundation; either version 2 of the License, or (at your
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
'*      /Filename:      SpellItemInformation
'*      /Description:   Includes functions for locating certain item and spell information
'+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
Imports System.Drawing
Imports System.Net
Imports System.Windows.Forms
Imports NCFramework.Framework.Extension
Imports libnc
Imports NCFramework.Framework.Logging
Imports NCFramework.Framework.Module

Namespace Framework.Functions

    Public Module SpellItemInformation
        Public Function GetGlyphIdByItemId(ByVal itemid As Integer) As Integer
            LogAppend("Loading GlyphId by ItemId " & itemid.ToString, "SpellItem_Information_GetGlyphIdByItemId", False)
            Dim xpacressource As String
            Try
                Select Case GlobalVariables.sourceExpansion
                    Case 3
                        xpacressource = libnc.My.Resources.glyphproperties_335
                    Case 4
                        xpacressource = libnc.My.Resources.glyphproperties_434
                    Case Else
                        xpacressource = libnc.My.Resources.glyphproperties_335
                End Select
                Dim client As New WebClient
                client.CheckProxy()
                Return _
                    TryInt(
                        SplitString(
                            client.DownloadString(
                                "http://www.wowhead.com/spell=" &
                                SplitString(xpacressource, "<entry>" & itemid.ToString & "</entry><spell>", "</spell>")),
                            ",""id"":", ",""level"""))
            Catch ex As Exception
                LogAppend(
                    "Error while loading GlyphId! -> Returning 0 -> Exception is: ###START###" & ex.ToString() & "###END###",
                    "SpellItem_Information_GetGlyphIdByItemId", False, True)
                Return 0
            End Try
        End Function

        Public Function GetIconByItemId(ByVal itemid As Integer) As Image
            If itemid = 0 Then Return Nothing
            LogAppend("Loading icon by ItemId " & itemid.ToString, "SpellItem_Information_GetIconByItemId", False)
            If GlobalVariables.offlineExtension = True Then

                If GlobalVariables.tempDisplayInfoTable Is Nothing Then
                    Try
                        GlobalVariables.tempDisplayInfoTable = New DataTable()
                        Dim stext As String
                        If My.Settings.language = "de" Then
                            stext = libnc.My.Resources.ItemDisplayInfo
                        Else
                            stext = libnc.My.Resources.ItemDisplayInfo 'todo
                        End If
                        Dim a() As String
                        Dim strArray As String()
                        a = Split(stext, vbNewLine)
                        For i = 0 To UBound(a)
                            strArray = a(i).Split(CChar(";"))
                            If i = 0 Then
                                For Each value As String In strArray
                                    GlobalVariables.tempDisplayInfoTable.Columns.Add(value.Trim())
                                Next
                            Else
                                GlobalVariables.tempDisplayInfoTable.Rows.Add(strArray)
                            End If
                        Next i
                    Catch ex As Exception
                        LogAppend("Error filling datatable! -> Exception is: ###START###" & ex.ToString() & "###END###",
                                  "SpellItem_Information_GetIconByItemId", False, True)
                        Return My.Resources.INV_Misc_QuestionMark
                    End Try
                End If
                Dim nameresult As String = Execute("itemid", itemid.ToString(), GlobalVariables.tempDisplayInfoTable)
                If nameresult = "-" Then
                    LogAppend("Entry not found -> Searching online", "SpellItem_Information_GetIconByItemId", False, True)
                    Dim client As New WebClient
                    client.CheckProxy()
                    Try
                        Dim itemContext As String =
                                client.DownloadString("http://www.wowhead.com/item=" & itemid.ToString & "&xml")
                        Try
                            Return _
                                libncadvanced.My.Resources.ResourceManager.GetObject(SplitString(itemContext,
                                                                                                 "<icon displayId=""" &
                                                                                                 SplitString(itemContext,
                                                                                                             "<icon displayId=""",
                                                                                                             """>") & """>",
                                                                                                 "</icon>"))
                        Catch ex As Exception
                            LogAppend("Icon not found -> Returning error image", "SpellItem_Information_GetIconByItemId",
                                      False, True)
                            Return My.Resources.INV_Misc_QuestionMark
                        End Try
                    Catch
                        LogAppend("Icon not found -> Returning error image", "SpellItem_Information_GetIconByItemId", False,
                                  True)
                        Return My.Resources.INV_Misc_QuestionMark
                    End Try
                Else
                    Try
                        Return libncadvanced.My.Resources.ResourceManager.GetObject(nameresult)
                    Catch ex As Exception
                        LogAppend("Icon not found -> Searching online", "SpellItem_Information_GetIconByItemId", False, True)
                        GoTo LookOnline
                    End Try
                End If
            Else
LookOnline:     Dim client As New WebClient
                client.CheckProxy()
                Try
                    Dim itemContext As String =
                            client.DownloadString("http://www.wowhead.com/item=" & itemid.ToString & "&xml")
                    Return _
                        LoadImageFromUrl(
                            "http://wow.zamimg.com/images/wow/icons/large/" &
                            (SplitString(itemContext,
                                         "<icon displayId=""" & SplitString(itemContext, "<icon displayId=""", """>") &
                                         """>", "</icon>")).ToLower() & ".jpg")
                Catch ex As Exception
                    LogAppend(
                        "Error while loading icon! -> Returning error image -> Exception is: ###START###" & ex.ToString() &
                        "###END###", "SpellItem_Information_GetIconByItemId", False, True)
                    Return My.Resources.INV_Misc_QuestionMark
                End Try
            End If
        End Function

        Public Function GetRarityByItemId(ByVal itemid As Integer) As Integer
            If itemid = 0 Then Return Nothing
            Dim client As New WebClient
            client.CheckProxy()
            Try
                LogAppend("Loading rarity by ItemId " & itemid.ToString, "SpellItem_Information_GetRarityByItemId", False)
                Dim itemContext As String = client.DownloadString("http://www.wowhead.com/item=" & itemid.ToString & "&xml")
                Return TryInt(SplitString(itemContext, "<quality id=""", """>"))
            Catch ex As Exception
                LogAppend(
                    "Error while loading rarity! -> Returning nothing -> Exception is: ###START###" & ex.ToString() &
                    "###END###", "SpellItem_Information_GetRarityByItemId", False, True)
                Return Nothing
            End Try
        End Function

        Public Function GetSlotByItemId(ByVal itemid As Integer) As Integer
            If itemid = 0 Then Return Nothing
            Dim client As New WebClient
            client.CheckProxy()
            Try
                LogAppend("Loading inventorySlot by ItemId " & itemid.ToString, "SpellItem_Information_GetSlotByItemId",
                          False)
                Dim itemContext As String = client.DownloadString("http://www.wowhead.com/item=" & itemid.ToString & "&xml")
                Return TryInt(SplitString(itemContext, "<inventorySlot id=""", """>"))
            Catch ex As Exception
                LogAppend(
                    "Error while loading inventorySlot! -> Returning nothing -> Exception is: ###START###" & ex.ToString() &
                    "###END###", "SpellItem_Information_GetSlotByItemId", False, True)
                Return Nothing
            End Try
        End Function

        Public Function GetSlotNameBySlotId(ByVal slotid As Integer) As Integer
            If slotid = 0 Then Return Nothing
            Select Case slotid
                Case 0
                    Return "head"
                Case 1
                    Return "neck"
                Case 2
                    Return "shoulder"
                Case 3
                    Return "shirt"
                Case 4
                    Return "chest"
                Case 5
                    Return "waist"
                Case 6
                    Return "legs"
                Case 7
                    Return "feet"
                Case 8
                    Return "wrists"
                Case 9
                    Return "hands"
                Case 10
                    Return "finger1"
                Case 11
                    Return "finger2"
                Case 12
                    Return "trinket1"
                Case 13
                    Return "trinket2"
                Case 14
                    Return "back"
                Case 15
                    Return "main"
                Case 16
                    Return "off"
                Case 17
                    Return Nothing
                    'slot 17 has been removed as of patch 5.0
                Case 18
                    Return "tabard"
                Case Else : Return Nothing
            End Select
        End Function

        Public Sub LoadWeaponType(ByVal itemid As Integer, ByVal tarSet As Integer)
            If Not itemid = 0 Then
                LogAppend("Loading weapon type of Item " & itemid.ToString, "SpellItem_Information_LoadWeaponType", False)
                Try
                    Dim client As New WebClient
                    client.CheckProxy()
                    Dim player As Character = GetCharacterSetBySetId(tarSet)
                    Dim excerpt As String =
                            SplitString(client.DownloadString("http://www.wowhead.com/item=" & itemid.ToString & "&xml"),
                                        "<subclass id=", "</subclass>")
                    Select Case True
                        Case excerpt.ToLower.Contains(" crossbow ") '5011
                            player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 5011})
                            player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 226})
                        Case excerpt.ToLower.Contains(" bow ")
                            player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 264})
                            player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 45})
                        Case excerpt.ToLower.Contains(" gun ")
                            player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 266})
                            player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 46})
                        Case excerpt.ToLower.Contains(" thrown ")
                            player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 2764})
                            player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 2567})
                            player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 176})
                        Case excerpt.ToLower.Contains(" wands ")
                            player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 5009})
                            player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 5019})
                            player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 228})
                        Case excerpt.ToLower.Contains(" sword ")
                            If excerpt.ToLower.Contains(" one-handed ") Then
                                player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 201})
                                player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 43})
                            Else
                                player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 201})
                                player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 43})
                                player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 202})
                                player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 55})
                            End If
                        Case excerpt.ToLower.Contains(" dagger ")
                            player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 1180})
                            player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 173})
                        Case excerpt.ToLower.Contains(" axe ")
                            If excerpt.ToLower.Contains(" one-handed ") Then
                                player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 196})
                                player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 44})
                            Else
                                player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 197})
                                player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 44})
                                player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 196})
                                player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 142})
                            End If
                        Case excerpt.ToLower.Contains(" mace ")
                            If excerpt.ToLower.Contains(" one-handed ") Then
                                player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 198})
                                player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 54})
                            Else
                                player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 54})
                                player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 198})
                                player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 160})
                                player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 199})
                            End If
                        Case excerpt.ToLower.Contains(" polearm ")
                            player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 200})
                            player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 229})
                        Case excerpt.ToLower.Contains(" staff ")
                            player.Spells.Add(New Spell With {.active = 1, .disabled = 0, .id = 227})
                            player.Skills.Add(New Skill With {.value = 100, .max = 100, .id = 136})
                        End Select
                Catch ex As Exception
                    LogAppend("Error while loading weapon type! -> Exception is: ###START###" & ex.ToString() & "###END###",
                              "SpellItem_Information_LoadWeaponType", False, True)
                End Try
            Else
            End If
        End Sub

        Public Function GetEffectNameByEffectId(ByVal effectid As Integer) As String
            LogAppend("Loading effectname by effectId: " & effectid.ToString,
                      "SpellItem_Information_GetEffectNameByEffectId", False)
            If GlobalVariables.effectname_dt Is Nothing Then GlobalVariables.effectname_dt = New DataTable()
            If GlobalVariables.effectname_dt.Rows.Count = 0 Then
                Try
                    GlobalVariables.effectname_dt.Clear()
                    GlobalVariables.effectname_dt = New DataTable()
                    Dim stext As String
                    If My.Settings.language = "de" Then
                        stext = libnc.My.Resources.enchant_name_de
                    Else
                        stext = libnc.My.Resources.enchant_name_en
                    End If
                    Dim a() As String
                    Dim strArray As String()
                    a = Split(stext, vbNewLine)
                    For i = 0 To UBound(a)
                        strArray = a(i).Split(CChar(";"))
                        If i = 0 Then
                            For Each value As String In strArray
                                GlobalVariables.effectname_dt.Columns.Add(value.Trim())
                            Next
                        Else
                            GlobalVariables.effectname_dt.Rows.Add(strArray)
                        End If
                    Next i
                Catch ex As Exception
                    LogAppend("Error filling datatable! -> Exception is: ###START###" & ex.ToString() & "###END###",
                              "SpellItem_Information_GetEffectNameByEffectId", False, True)
                    Return "Error loading effectname"
                End Try
            End If
            Dim nameresult As String = Execute("effectid", effectid.ToString(), GlobalVariables.effectname_dt)
            If nameresult = "-" Then
                LogAppend("Entry not found -> Returning error message", "SpellItem_Information_GetEffectNameByEffectId",
                          False, True)
                Return "Error loading effect name"
            Else
                Return nameresult
            End If
        End Function

        Public Function GetNameOfItem(ByVal itemid As String) As String
            LogAppend("Loading name of item: " & itemid.ToString, "SpellItem_Information_getNameOfItem", False)
            If Not itemid = Nothing Then
                If itemid.Length > 1 Then
                    Dim client As New WebClient
                    client.CheckProxy()
                    Try
                        If My.Settings.language = "de" Then
                            If GlobalVariables.tempItemSparseDE Is Nothing Then
                                Try
                                    LogAppend("Filling tempFactionTable", "SpellItem_Information_getNameOfItem", False)
                                    GlobalVariables.tempItemSparseDE = New DataTable()
                                    Dim stext As String = libnc.My.Resources.dbc_itemsparse_de
                                    Dim a() As String
                                    Dim strArray As String()
                                    a = Split(stext, vbNewLine)
                                    For i = 0 To UBound(a)
                                        strArray = a(i).Split(CChar(";"))
                                        If i = 0 Then
                                            For Each value As String In strArray
                                                GlobalVariables.tempItemSparseDE.Columns.Add(value.Trim())
                                            Next
                                        Else
                                            GlobalVariables.tempItemSparseDE.Rows.Add(strArray)
                                        End If
                                    Next i
                                Catch ex As Exception
                                    LogAppend(
                                        "Error filling datatable! -> Exception is: ###START###" & ex.ToString() &
                                        "###END###", "SpellItem_Information_getNameOfItem", False, True)
                                    Return "Error"
                                End Try
                            End If
                            Dim nameresult As String = Execute("id", itemid.ToString(), GlobalVariables.tempItemSparseDE,
                                                               100)
                            If nameresult = "-" Then
                                LogAppend("Entry not found -> Returning error message",
                                          "SpellItem_Information_getNameOfItem", False, True)
                                Dim clString As String =
                                        client.DownloadString("http://de.wowhead.com/item=" & itemid.ToString() & "&xml")
                                Return SplitString(clString, "<name><![CDATA[", "]]></name>")
                            Else
                                Return nameresult
                            End If

                        Else
                            Dim clString As String =
                                    client.DownloadString("http://wowhead.com/item=" & itemid.ToString() & "&xml")
                            Return SplitString(clString, "<name><![CDATA[", "]]></name>")
                        End If
                    Catch ex As Exception
                        LogAppend(
                            "Error while loading item name! -> Exception is: ###START###" & ex.ToString() & "###END###",
                            "SpellItem_Information_getNameOfItem", False, True)
                        Return "Error loading item name"
                    End Try
                End If
            End If
            LogAppend("ItemId is nothing -> Returning error", "SpellItem_Information_getNameOfItem", False, True)
            Return "Error loading item name"
        End Function

        Public Function GetGemEffectName(ByVal socketid As Integer) As String
            LogAppend("Loading effect name of gem: " & socketid.ToString, "SpellItem_Information_GetGemEffectName", False)
            Try
                Dim client As New WebClient
                client.CheckProxy()
                Dim effectname As String
                If My.Settings.language = "de" Then
                    effectname = client.DownloadString("http://de.wowhead.com/item=" & socketid.ToString & "&xml")
                Else
                    effectname = client.DownloadString("http://www.wowhead.com/item=" & socketid.ToString & "&xml")
                End If
                effectname = SplitString(effectname, "<span class=""q1"">", "</span>")

                If effectname.Contains("<a href") Then
                    Try
                        effectname = effectname.Replace("<a href=""" & SplitString(effectname, "<a href=""", """>") & """>",
                                                        "")
                        effectname = effectname.Replace("</a>", "")
                        Return effectname
                    Catch ex As Exception
                        Return effectname
                    End Try
                Else
                    Return effectname
                End If
            Catch ex As Exception
                LogAppend("Error while loading effect name! -> Exception is: ###START###" & ex.ToString() & "###END###",
                          "SpellItem_Information_GetGemEffectName", False, True)
                Return "Error loading effect name"
            End Try
        End Function

        Private Function Execute(ByVal field As String, ByVal isvalue As String, ByVal tempdatatable As DataTable,
                                 Optional secfield As Integer = 1) As String
            LogAppend("Browsing datatale (field = " & field & " // value = " & isvalue & ")",
                      "SpellItem_Information_Execute", False)
            Try
                Dim foundRows() As DataRow
                foundRows = tempdatatable.Select(field & " = '" & isvalue & "'")
                If foundRows.Length = 0 Then
                    Return "-"
                Else
                    Dim i As Integer
                    Dim tmpreturn As String = "-"
                    For i = 0 To foundRows.GetUpperBound(0)
                        tmpreturn = (foundRows(i)(secfield)).ToString
                    Next i
                    Return tmpreturn
                End If
            Catch ex As Exception
                LogAppend("Error while browsing datatable! -> Exception is: ###START###" & ex.ToString() & "###END###",
                          "SpellItem_Information_Execute", False, True)
                Return "-"
            End Try
        End Function

        Public Function GetFactionNameById(ByVal facid As Integer) As String
            LogAppend("Loading faction name of id: " & facid.ToString, "SpellItem_Information_GetFactionNameById", False)
            If GlobalVariables.tempFactionTable Is Nothing Then
                Try
                    LogAppend("Filling tempFactionTable", "SpellItem_Information_GetFactionNameById", False)
                    GlobalVariables.tempFactionTable = New DataTable()
                    Dim stext As String
                    If My.Settings.language = "de" Then
                        stext = libnc.My.Resources.FactionDE
                    Else
                        stext = libnc.My.Resources.FactionDE 'todo
                    End If
                    Dim a() As String
                    Dim strArray As String()
                    a = Split(stext, vbNewLine)
                    For i = 0 To UBound(a)
                        strArray = a(i).Split(CChar(";"))
                        If i = 0 Then
                            For Each value As String In strArray
                                GlobalVariables.tempFactionTable.Columns.Add(value.Trim())
                            Next
                        Else
                            GlobalVariables.tempFactionTable.Rows.Add(strArray)
                        End If
                    Next i
                Catch ex As Exception
                    LogAppend("Error filling datatable! -> Exception is: ###START###" & ex.ToString() & "###END###",
                              "SpellItem_Information_GetFactionNameById", False, True)
                    Return "Error"
                End Try
            End If
            Dim nameresult As String = Execute("factionid", facid.ToString(), GlobalVariables.tempFactionTable, 23)
            If nameresult = "-" Then
                LogAppend("Entry not found -> Returning error message", "SpellItem_Information_GetFactionNameById", False,
                          True)
                Return "Error loading faction name"
            Else
                Return nameresult
            End If
        End Function

        Public Function GetAvNameById(ByVal avid As Integer) As String

            LogAppend("Loading av name of id: " & avid.ToString, "SpellItem_Information_GetAvNameById", False)
            If GlobalVariables.tempAvTable Is Nothing Then
                Try
                    LogAppend("Filling tempAvTable", "SpellItem_Information_GetAvNameById", False)
                    GlobalVariables.tempAvTable = New DataTable()
                    Dim stext As String
                    If My.Settings.language = "de" Then
                        stext = libnc.My.Resources.av_de
                    Else
                        stext = libnc.My.Resources.av_de 'todo
                    End If
                    Dim a() As String
                    Dim strArray As String()
                    a = Split(stext, vbNewLine)
                    For i = 0 To UBound(a)
                        strArray = a(i).Split(CChar(";"))
                        If i = 0 Then
                            For Each value As String In strArray
                                GlobalVariables.tempAvTable.Columns.Add(value.Trim())
                            Next
                        Else
                            GlobalVariables.tempAvTable.Rows.Add(strArray)
                        End If
                    Next i
                Catch ex As Exception
                    LogAppend("Error filling datatable! -> Exception is: ###START###" & ex.ToString() & "###END###",
                              "SpellItem_Information_GetAvNameById", False, True)
                    Return "Error"
                End Try
            End If
            Dim nameresult As String = Execute("avid", avid.ToString(), GlobalVariables.tempAvTable)
            If nameresult = "-" Then
                LogAppend("Entry not found -> Returning error message", "SpellItem_Information_GetAvNameById", False, True)
                Return "Error loading av name"
            Else
                Return nameresult
            End If
        End Function

        Public Function GetAvCategoryById(ByVal avid As Integer, Optional subcat As Boolean = False) As String
            LogAppend("Loading av category of id: " & avid.ToString, "SpellItem_Information_GetAvCategoryById", False)
            If GlobalVariables.tempAvCatTable Is Nothing Then
                Try
                    GlobalVariables.tempAvCatTable = New DataTable()
                    Dim stext As String
                    If My.Settings.language = "de" Then
                        stext = libnc.My.Resources.avcat_de
                    Else
                        stext = libnc.My.Resources.avcat_de 'todo
                    End If
                    Dim a() As String
                    Dim strArray As String()
                    a = Split(stext, vbNewLine)
                    For i = 0 To UBound(a)
                        strArray = a(i).Split(CChar(";"))
                        If i = 0 Then
                            For Each value As String In strArray
                                GlobalVariables.tempAvCatTable.Columns.Add(value.Trim())
                            Next
                        Else
                            GlobalVariables.tempAvCatTable.Rows.Add(strArray)
                        End If
                    Next i
                Catch ex As Exception
                    LogAppend("Error filling datatable! -> Exception is: ###START###" & ex.ToString() & "###END###",
                              "SpellItem_Information_GetAvCategoryById", False, True)
                    Return "Error"
                End Try
            End If
            Dim catid As String = Execute("avid", avid.ToString(), GlobalVariables.tempAvTable, 3)
            If catid = "-" Then
                LogAppend("Entry not found -> Returning error message", "SpellItem_Information_GetAvCategoryById", False,
                          True)
                Return "Error"
            Else
                Dim subcatname As String = Execute("catid", catid, GlobalVariables.tempAvCatTable, 2)
                If subcatname = "-" Then
                    LogAppend("Entry not found -> Returning error message", "SpellItem_Information_GetAvCategoryById", False,
                              True)
                    Return "Error"
                Else
                    If subcat Then
                        Return subcatname
                    End If
                    If catid = "-1" Then Return subcatname
                    Dim maincatid As String = Execute("catid", catid, GlobalVariables.tempAvCatTable, 1)
                    If maincatid = "-" Then
                        LogAppend("Entry not found -> Returning error message", "SpellItem_Information_GetAvCategoryById",
                                  False, True)
                        Return "Error"
                    Else
                        If maincatid = "-1" Then Return subcatname
                        Dim maincatname As String = Execute("catid", maincatid, GlobalVariables.tempAvCatTable, 2)
                        If maincatname = "-" Then
                            LogAppend("Entry not found -> Returning error message",
                                      "SpellItem_Information_GetAvCategoryById", False, True)
                            Return "Error"
                        Else
                            Return maincatname
                        End If
                    End If
                End If
            End If
        End Function

        Public Function GetAvCategoryIdByAvId(ByVal avid As Integer) As Integer
            LogAppend("Loading av category id of av id: " & avid.ToString, "SpellItem_Information_GetAvCategoryIdByAvId",
                      False)
            If GlobalVariables.tempAvTable Is Nothing Then
                Try
                    GlobalVariables.tempAvTable = New DataTable()
                    Dim stext As String
                    If My.Settings.language = "de" Then
                        stext = libnc.My.Resources.av_de
                    Else
                        stext = libnc.My.Resources.av_de 'todo
                    End If
                    Dim a() As String
                    Dim strArray As String()
                    a = Split(stext, vbNewLine)
                    For i = 0 To UBound(a)
                        strArray = a(i).Split(CChar(";"))
                        If i = 0 Then
                            For Each value As String In strArray
                                GlobalVariables.tempAvTable.Columns.Add(value.Trim())
                            Next
                        Else
                            GlobalVariables.tempAvTable.Rows.Add(strArray)
                        End If
                    Next i
                Catch ex As Exception
                    LogAppend("Error filling datatable! -> Exception is: ###START###" & ex.ToString() & "###END###",
                              "SpellItem_Information_GetAvCategoryIdByAvId", False, True)
                    Return 0
                End Try
            End If
            If GlobalVariables.tempAvCatTable Is Nothing Then
                Try
                    GlobalVariables.tempAvCatTable = New DataTable()
                    Dim stext As String
                    If My.Settings.language = "de" Then
                        stext = libnc.My.Resources.avcat_de
                    Else
                        stext = libnc.My.Resources.avcat_de 'todo
                    End If
                    Dim a() As String
                    Dim strArray As String()
                    a = Split(stext, vbNewLine)
                    For i = 0 To UBound(a)
                        strArray = a(i).Split(CChar(";"))
                        If i = 0 Then
                            For Each value As String In strArray
                                GlobalVariables.tempAvCatTable.Columns.Add(value.Trim())
                            Next
                        Else
                            GlobalVariables.tempAvCatTable.Rows.Add(strArray)
                        End If
                    Next i
                Catch ex As Exception
                    LogAppend("Error filling datatable! -> Exception is: ###START###" & ex.ToString() & "###END###",
                              "SpellItem_Information_GetAvCategoryIdByAvId", False, True)
                    Return 0
                End Try
            End If
            Dim catid As String = Execute("avid", avid.ToString(), GlobalVariables.tempAvTable, 3)
            If catid = "-" Then
                LogAppend("Entry not found -> Returning error message", "SpellItem_Information_GetAvCategoryIdByAvId", False,
                          True)
                Return 0
            Else
                Dim subcatname As String = Execute("catid", catid, GlobalVariables.tempAvCatTable, 2)
                If subcatname = "-" Then
                    LogAppend("Entry not found -> Returning error message", "SpellItem_Information_GetAvCategoryIdByAvId",
                              False, True)
                    Return 0
                Else
                    Return TryInt(catid)
                End If
            End If
        End Function

        Public Function GetMainAvCatIdByAvId(ByVal avId As Integer) As Integer
            LogAppend("Loading av main category of id: " & avId.ToString, "SpellItem_Information_GetMainAvCatIdByAvId",
                      False)
            If GlobalVariables.tempAvTable Is Nothing Then
                Try
                    GlobalVariables.tempAvTable = New DataTable()
                    Dim stext As String
                    If My.Settings.language = "de" Then
                        stext = libnc.My.Resources.av_de
                    Else
                        stext = libnc.My.Resources.av_de 'todo
                    End If
                    Dim a() As String
                    Dim strArray As String()
                    a = Split(stext, vbNewLine)
                    For i = 0 To UBound(a)
                        strArray = a(i).Split(CChar(";"))
                        If i = 0 Then
                            For Each value As String In strArray
                                GlobalVariables.tempAvTable.Columns.Add(value.Trim())
                            Next
                        Else
                            GlobalVariables.tempAvTable.Rows.Add(strArray)
                        End If
                    Next i
                Catch ex As Exception
                    LogAppend("Error filling datatable! -> Exception is: ###START###" & ex.ToString() & "###END###",
                              "SpellItem_Information_GetMainAvCatIdByAvId", False, True)
                    Return 0
                End Try
            End If
            If GlobalVariables.tempAvCatTable Is Nothing Then
                Try
                    GlobalVariables.tempAvCatTable = New DataTable()
                    Dim stext As String
                    If My.Settings.language = "de" Then
                        stext = libnc.My.Resources.avcat_de
                    Else
                        stext = libnc.My.Resources.avcat_de 'todo
                    End If
                    Dim a() As String
                    Dim strArray As String()
                    a = Split(stext, vbNewLine)
                    For i = 0 To UBound(a)
                        strArray = a(i).Split(CChar(";"))
                        If i = 0 Then
                            For Each value As String In strArray
                                GlobalVariables.tempAvCatTable.Columns.Add(value.Trim())
                            Next
                        Else
                            GlobalVariables.tempAvCatTable.Rows.Add(strArray)
                        End If
                    Next i
                Catch ex As Exception
                    LogAppend("Error filling datatable! -> Exception is: ###START###" & ex.ToString() & "###END###",
                              "SpellItem_Information_GetMainAvCatIdByAvId", False, True)
                    Return 0
                End Try
            End If
            Dim catid As String = Execute("avid", avId.ToString(), GlobalVariables.tempAvTable, 3)
            If catid = "-" Then
                LogAppend("Entry not found -> Returning error message", "SpellItem_Information_GetMainAvCatIdByAvId", False,
                          True)
                Return 0
            Else
                Dim subcatname As String = Execute("catid", catid, GlobalVariables.tempAvCatTable, 2)
                If subcatname = "-" Then
                    LogAppend("Entry not found -> Returning error message", "SpellItem_Information_GetMainAvCatIdByAvId",
                              False, True)
                    Return 0
                Else
                    If catid = "-1" Then Return TryInt(catid)
                    Dim maincatid As String = Execute("catid", catid, GlobalVariables.tempAvCatTable, 1)
                    If maincatid = "-" Then
                        LogAppend("Entry not found -> Returning error message", "SpellItem_Information_GetMainAvCatIdByAvId",
                                  False, True)
                        Return 0
                    Else
                        If maincatid = "-1" Then Return TryInt(catid)
                        Dim maincatname As String = Execute("catid", maincatid, GlobalVariables.tempAvCatTable, 2)
                        If maincatname = "-" Then
                            LogAppend("Entry not found -> Returning error message",
                                      "SpellItem_Information_GetMainAvCatIdByAvId", False, True)
                            Return 0
                        Else
                            Return TryInt(maincatid)
                        End If
                    End If
                End If
            End If
        End Function

        Public Function GetAvDescriptionById(ByVal avid As Integer) As String
            LogAppend("Loading av description of id: " & avid.ToString, "SpellItem_Information_GetAvDescriptionById", False)
            Dim desc As String = Execute("avid", avid.ToString(), GlobalVariables.tempAvTable, 2)
            If desc = "-" Then
                LogAppend("Entry not found -> Returning error message", "SpellItem_Information_GetAvDescriptionById", False,
                          True)
                Return "Error"
            Else
                Return desc
            End If
        End Function

        Public Function GetQuestNameById(ByVal questid As Integer) As String
            LogAppend("Loading quest name of id: " & questid.ToString, "SpellItem_Information_GetQuestNameById", False)
            If GlobalVariables.tempQuestNameTable Is Nothing Then
                Try
                    LogAppend("Filling tempAvTable", "SpellItem_Information_GetAvNameById", False)
                    GlobalVariables.tempQuestNameTable = New DataTable()
                    Dim stext As String
                    If My.Settings.language = "de" Then
                        stext = libnc.My.Resources.questname
                    Else
                        stext = libnc.My.Resources.questname 'todo
                    End If
                    Dim a() As String
                    Dim strArray As String()
                    a = Split(stext, vbNewLine)
                    For i = 0 To UBound(a)
                        strArray = a(i).Split(CChar(";"))
                        If i = 0 Then
                            For Each value As String In strArray
                                GlobalVariables.tempQuestNameTable.Columns.Add(value.Trim())
                            Next
                        Else
                            GlobalVariables.tempQuestNameTable.Rows.Add(strArray)
                        End If
                    Next i
                    Application.DoEvents()
                Catch ex As Exception
                    LogAppend("Error filling datatable! -> Exception is: ###START###" & ex.ToString() & "###END###",
                              "SpellItem_Information_GetQuestNameById", False, True)
                    Return "error"
                End Try
            End If

            Dim desc As String = Execute("Id", questid.ToString(), GlobalVariables.tempQuestNameTable)
            If desc = "-" Then
                LogAppend("Entry not found -> Returning error message", "SpellItem_Information_GetQuestNameById", False,
                          True)
                Return "error"
            Else
                Return desc
            End If
        End Function

        Public Function GetAvIconById(ByVal avId As Integer) As Image
            LogAppend("Loading achievement icon of id: " & avId.ToString, "SpellItem_Information_GetAvIconById", False)
            Try
                If GlobalVariables.tempAvIconTable Is Nothing Then
                    Try
                        LogAppend("Filling tempAvIconTable", "SpellItem_Information_GetAvIconById", False)
                        GlobalVariables.tempAvIconTable = New DataTable()
                        Dim stext As String = libnc.My.Resources.avicon
                        Dim a() As String
                        Dim strArray As String()
                        a = Split(stext, vbNewLine)
                        For i = 0 To UBound(a)
                            strArray = a(i).Split(CChar(";"))
                            If i = 0 Then
                                For Each value As String In strArray
                                    GlobalVariables.tempAvIconTable.Columns.Add(value.Trim())
                                Next
                            Else
                                GlobalVariables.tempAvIconTable.Rows.Add(strArray)
                            End If
                        Next i
                        Application.DoEvents()
                    Catch ex As Exception
                        LogAppend("Error filling datatable! -> Exception is: ###START###" & ex.ToString() & "###END###",
                                  "SpellItem_Information_GetAvIconById", False, True)
                    End Try
                End If
                Dim picname As String = ""
                If Not GlobalVariables.tempAvIconTable Is Nothing Then
                    Dim desc As String = Execute("id", avId.ToString(), GlobalVariables.tempAvIconTable, 2)
                    If desc = "-" Then
                        LogAppend("Entry not found", "SpellItem_Information_GetAvIconById", False, True)
                        picname = ""
                    Else
                        picname = desc
                    End If
                End If
                If picname = "" Then Return My.Resources.INV_Misc_QuestionMark
                Dim pic As Image = libncadvanced.My.Resources.ResourceManager.GetObject(picname.ToLower())
                If pic Is Nothing Then
                    LogAppend("Loading icon from web!", "SpellItem_Information_GetAvIconById", False)
                    Dim onlinePic As Image =
                            LoadImageFromUrl("http://wow.zamimg.com/images/wow/icons/large/" & picname.ToLower() & ".jpg")
                    If onlinePic Is Nothing Then
                        Return My.Resources.INV_Misc_QuestionMark
                    Else
                        Return onlinePic
                    End If
                Else
                    LogAppend("Loaded icon from library", "SpellItem_Information_GetAvIconById", False)
                    Return pic
                End If

            Catch ex As Exception
                LogAppend(
                    "Error while loading achievement icon of id: " & avId.ToString & " // ErrorMsg is: " & ex.ToString(),
                    "SpellItem_Information_GetAvIconById", False, True)
                Return My.Resources.INV_Misc_QuestionMark
            End Try
        End Function

        Public Function GetAvIdListByMainCat(ByVal maincatid As Integer) As List(Of Integer)
            LogAppend("Loading av id list by main category id: " & maincatid.ToString,
                      "SpellItem_Information_GetAvIdListByMainCat", False)
            If GlobalVariables.tempAvMainCatTable Is Nothing Then
                Try
                    GlobalVariables.tempAvMainCatTable = New DataTable()
                    Dim stext As String
                    If My.Settings.language = "de" Then
                        stext = libnc.My.Resources.avmaincat
                    Else
                        stext = libnc.My.Resources.avmaincat 'todo
                    End If
                    Dim a() As String
                    Dim strArray As String()
                    a = Split(stext, vbNewLine)
                    For i = 0 To UBound(a)
                        strArray = a(i).Split(CChar(";"))
                        If i = 0 Then
                            For Each value As String In strArray
                                GlobalVariables.tempAvMainCatTable.Columns.Add(value.Trim())
                            Next
                        Else
                            GlobalVariables.tempAvMainCatTable.Rows.Add(strArray)
                        End If
                    Next i
                Catch ex As Exception
                    LogAppend("Error filling datatable! -> Exception is: ###START###" & ex.ToString() & "###END###",
                              "SpellItem_Information_GetAvIdListByMainCat", False, True)
                    Return Nothing
                End Try
            End If
            Dim desc() As DataRow = GetAll(maincatid)
            If desc Is Nothing Then
                LogAppend("Entry not found -> Returning error message", "SpellItem_Information_GetAvIdListByMainCat", False,
                          True)
                Return Nothing
            Else
                Dim tempIdList As New List(Of Integer)
                Try
                    For i = 0 To desc.Length
                        tempIdList.Add(TryInt(desc(i)(0)))
                    Next i
                Catch ex As Exception

                End Try
                Return tempIdList
            End If
        End Function

        Private Function GetAll(ByVal maincatid As Integer) As DataRow()
            'logAppend("Browsing datatale (field = " & field & " // value = " & isvalue & ")", "SpellItem_Information_Execute", False)

            Try
                Dim foundRows() As DataRow
                foundRows = GlobalVariables.tempAvMainCatTable.Select("maincatid = '" & maincatid.ToString() & "'")
                If foundRows.Length = 0 Then
                    Return Nothing
                Else
                    Return foundRows
                End If
            Catch ex As Exception
                'logAppend("Error while browsing datatable! -> Exception is: ###START###" & ex.ToString() & "###END###", "SpellItem_Information_Execute", False, True)
                Return Nothing
            End Try
        End Function

        Public Function GetSpellNameById(ByVal spellId As Integer) As String
            LogAppend("Loading spell name by id: " & spellId.ToString, "SpellItem_Information_GetSpellNameById", False)
            Dim infoHandler As New InformationHandler
            Return infoHandler.GetSpellNameById(spellId, My.Settings.language)
        End Function

        Public Function GetSkillNameById(ByVal skillId As Integer) As String
            LogAppend("Loading skill name by id: " & skillId.ToString, "SpellItem_Information_GetSkillNameById", False)
            Dim infoHandler As New InformationHandler
            Return infoHandler.GetSkillNameById(skillId, My.Settings.language)
        End Function
    End Module
End Namespace