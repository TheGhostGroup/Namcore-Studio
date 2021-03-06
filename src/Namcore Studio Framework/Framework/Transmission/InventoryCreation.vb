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
'*      /Filename:      InventoryCreation
'*      /Description:   Includes functions for adding the items of a specific
'*                      character
'+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
Imports NCFramework.Framework.Functions
Imports NCFramework.Framework.Database
Imports NCFramework.Framework.Logging
Imports NCFramework.Framework.Modules

Namespace Framework.Transmission
    Public Class InventoryCreation
        Public Sub AddCharacterInventory(ByVal player As Character, Optional charguid As Integer = 0)
            If charguid = 0 Then charguid = player.Guid
            LogAppend(
                "Adding inventory items to character: " & charguid.ToString(),
                "InventoryCreation_AddCharacterInventory", True)
            Try
                Select Case GlobalVariables.targetCore
                    Case Modules.Core.ARCEMU
                        CreateAtArcemu(charguid, player)
                    Case Modules.Core.TRINITY
                        CreateAtTrinity(charguid, player)
                    Case Modules.Core.MANGOS
                        CreateAtMangos(charguid, player)
                End Select
            Catch ex As Exception
                LogAppend("Exception occured: " & ex.ToString(),
                          "InventoryCreation_AddCharacterInventory", False, True)
            End Try
        End Sub

        Private Sub CreateAtArcemu(ByVal characterguid As Integer, ByVal player As Character)
            LogAppend("Adding inventory at arcemu", "InventoryCreation_createAtArcemu", False)
            Dim bagexist As List(Of String) = New List(Of String)
            Dim bagstring As String = ""
            bagexist.Clear()
            If Not player.InventoryZeroItems Is Nothing Then
                For Each inventoryItem As Item In player.InventoryZeroItems
                    Dim newguid As String =
                            ((TryInt(
                                runSQLCommand_characters_string(
                                    "SELECT " & GlobalVariables.targetStructure.itmins_guid_col(0) & " FROM " &
                                    GlobalVariables.targetStructure.item_instance_tbl(0) & " WHERE " &
                                    GlobalVariables.targetStructure.itmins_guid_col(0) &
                                    "=(SELECT MAX(" & GlobalVariables.targetStructure.itmins_guid_col(0) & ") FROM " &
                                    GlobalVariables.targetStructure.item_instance_tbl(0) & ")")) + 1)).ToString

                    runSQLCommand_characters_string(
                        "INSERT INTO " & GlobalVariables.targetStructure.item_instance_tbl(0) & " ( " &
                        GlobalVariables.targetStructure.itmins_guid_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_ownerGuid_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_itemEntry_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_count_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_container_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_slot_col(0) &
                        " ) VALUES ( '" &
                        newguid & "', '" &
                        characterguid.ToString() & "', '" &
                        inventoryItem.Id.ToString() & "', '" &
                        inventoryItem.Count.ToString() & "', '" &
                        inventoryItem.Container.ToString() & "', '" &
                        inventoryItem.Slot.ToString() & "' )")
                    Select Case inventoryItem.Slot
                        Case 19, 20, 21, 22, 67, 68, 69, 70, 71, 72, 73
                            '//Item is a bag and has to be registered
                            bagstring = bagstring & "oldguid:" & inventoryItem.Guid.ToString() & ";slot:" & newguid &
                                        ";"
                    End Select
                    Dim mEnchCreation As New EnchantmentsCreation
                    mEnchCreation.SetItemEnchantments(player,
                                                      New Item _
                                                         With {.Id = inventoryItem.Id,
                                                         .Socket1Effectid = inventoryItem.Socket1Effectid,
                                                         .Socket2Effectid = inventoryItem.Socket2Effectid,
                                                         .Socket3Effectid = inventoryItem.Socket3Effectid,
                                                         .EnchantmentEffectid = inventoryItem.EnchantmentEffectid},
                                                      TryInt(newguid), GlobalVariables.targetCore,
                                                      GlobalVariables.targetStructure)
                Next
            End If
            If Not player.InventoryItems Is Nothing Then
                For Each inventoryItem As Item In player.InventoryItems
                    Dim newguid As String =
                            ((TryInt(
                                runSQLCommand_characters_string(
                                    "SELECT " & GlobalVariables.targetStructure.itmins_guid_col(0) & " FROM " &
                                    GlobalVariables.targetStructure.item_instance_tbl(0) & " WHERE " &
                                    GlobalVariables.targetStructure.itmins_guid_col(0) &
                                    "=(SELECT MAX(" & GlobalVariables.targetStructure.itmins_guid_col(0) & ") FROM " &
                                    GlobalVariables.targetStructure.item_instance_tbl(0) & ")")) + 1)).ToString
                    If inventoryItem.Container = Nothing Then
                        inventoryItem.Container = TryInt(SplitString(bagstring,
                                                                     "oldguid:" & inventoryItem.Bagguid.ToString() &
                                                                     ";slot:",
                                                                     ";"))

                    End If
                    runSQLCommand_characters_string(
                        "INSERT INTO " & GlobalVariables.targetStructure.item_instance_tbl(0) & " ( " &
                        GlobalVariables.targetStructure.itmins_guid_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_ownerGuid_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_itemEntry_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_count_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_container_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_slot_col(0) &
                        " ) VALUES ( '" &
                        newguid & "', '" &
                        characterguid.ToString() & "', '" &
                        inventoryItem.Id.ToString() & "', '" &
                        inventoryItem.Count.ToString() & "', '" &
                        inventoryItem.Container.ToString() & "', '" &
                        inventoryItem.Slot.ToString() & "' )")
                    Dim mEnchCreation As New EnchantmentsCreation
                    mEnchCreation.SetItemEnchantments(player,
                                                      New Item _
                                                         With {.Id = inventoryItem.Id,
                                                         .Socket1Effectid = inventoryItem.Socket1Effectid,
                                                         .Socket2Effectid = inventoryItem.Socket2Effectid,
                                                         .Socket3Effectid = inventoryItem.Socket3Effectid,
                                                         .EnchantmentEffectid = inventoryItem.EnchantmentEffectid},
                                                      TryInt(newguid), GlobalVariables.targetCore,
                                                      GlobalVariables.targetStructure)
                Next
            End If
        End Sub

        Private Sub CreateAtTrinity(ByVal characterguid As Integer, ByVal player As Character)
            LogAppend("Adding inventory at trinity", "InventoryCreation_createAtTrinity", False)
            Dim bagexist As List(Of String) = New List(Of String)
            Dim bagstring As String = ""
            bagexist.Clear()
            If Not player.InventoryZeroItems Is Nothing Then
                For Each inventoryItem As Item In player.InventoryZeroItems
                    Dim newguid As String =
                            ((TryInt(
                                runSQLCommand_characters_string(
                                    "SELECT " & GlobalVariables.targetStructure.itmins_guid_col(0) & " FROM " &
                                    GlobalVariables.targetStructure.item_instance_tbl(0) & " WHERE " &
                                    GlobalVariables.targetStructure.itmins_guid_col(0) &
                                    "=(SELECT MAX(" & GlobalVariables.targetStructure.itmins_guid_col(0) & ") FROM " &
                                    GlobalVariables.targetStructure.item_instance_tbl(0) & ")")) + 1)).ToString

                    runSQLCommand_characters_string(
                        "INSERT INTO " & GlobalVariables.targetStructure.item_instance_tbl(0) & " ( " &
                        GlobalVariables.targetStructure.itmins_guid_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_itemEntry_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_ownerGuid_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_count_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_durability_col(0) &
                        " ) VALUES ( '" &
                        newguid & "', '" & inventoryItem.Id & "', '" & characterguid.ToString() &
                        "', '" & inventoryItem.Count.ToString() & "', '1000' )")

                    runSQLCommand_characters_string(
                        "INSERT INTO " & GlobalVariables.targetStructure.character_inventory_tbl(0) & " ( " &
                        GlobalVariables.targetStructure.invent_guid_col(0) & ", " &
                        GlobalVariables.targetStructure.invent_bag_col(0) &
                        ", `" & GlobalVariables.targetStructure.invent_slot_col(0) & "`, `" &
                        GlobalVariables.targetStructure.invent_item_col(0) & "` ) VALUES ( '" &
                        characterguid.ToString() &
                        "', '" &
                        inventoryItem.Bag.ToString() & "', '" & inventoryItem.Slot.ToString() & "', '" & newguid &
                        "')")
                    Select Case inventoryItem.Slot
                        Case 19, 20, 21, 22, 67, 68, 69, 70, 71, 72, 73
                            '// Item is a bag and has to be registered
                            bagstring = bagstring & "oldguid:" & inventoryItem.Guid.ToString() & ";newguid:" & newguid &
                                        ";"
                    End Select
                    Dim mEnchCreation As New EnchantmentsCreation
                    mEnchCreation.SetItemEnchantments(player,
                                                      New Item _
                                                         With {.Id = inventoryItem.Id,
                                                         .Socket1Effectid = inventoryItem.Socket1Effectid,
                                                         .Socket2Effectid = inventoryItem.Socket2Effectid,
                                                         .Socket3Effectid = inventoryItem.Socket3Effectid,
                                                         .EnchantmentEffectid = inventoryItem.EnchantmentEffectid},
                                                      TryInt(newguid), GlobalVariables.targetCore,
                                                      GlobalVariables.targetStructure)
                Next
            End If
            If Not player.InventoryItems Is Nothing Then
                For Each inventoryItem As Item In player.InventoryItems
                    Dim newguid As String =
                            ((TryInt(
                                runSQLCommand_characters_string(
                                    "SELECT " & GlobalVariables.targetStructure.itmins_guid_col(0) & " FROM " &
                                    GlobalVariables.targetStructure.item_instance_tbl(0) & " WHERE " &
                                    GlobalVariables.targetStructure.itmins_guid_col(0) &
                                    "=(SELECT MAX(" & GlobalVariables.targetStructure.itmins_guid_col(0) & ") FROM " &
                                    GlobalVariables.targetStructure.item_instance_tbl(0) & ")")) + 1)).ToString
                    Dim newbagguid As String = SplitString(bagstring,
                                                           "oldguid:" & inventoryItem.Bagguid.ToString() & ";newguid:",
                                                           ";")
                    runSQLCommand_characters_string(
                        "INSERT INTO " & GlobalVariables.targetStructure.item_instance_tbl(0) & " ( " &
                        GlobalVariables.targetStructure.itmins_guid_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_itemEntry_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_ownerGuid_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_count_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_durability_col(0) & " ) VALUES ( '" &
                        newguid & "', '" & inventoryItem.Id.ToString() & "', '" & characterguid.ToString() &
                        "', '" & inventoryItem.Count.ToString() & "', '1000' )")
                    runSQLCommand_characters_string(
                        "INSERT INTO " & GlobalVariables.targetStructure.character_inventory_tbl(0) & " ( " &
                        GlobalVariables.targetStructure.invent_guid_col(0) & ", " &
                        GlobalVariables.targetStructure.invent_bag_col(0) & ", `" &
                        GlobalVariables.targetStructure.invent_slot_col(0) & "`, `" &
                        GlobalVariables.targetStructure.invent_item_col(0) & "` ) VALUES ( '" & characterguid.ToString() &
                        "', '" &
                        newbagguid & "', '" & inventoryItem.Slot.ToString() & "', '" & newguid & "')")
                    Dim mEnchCreation As New EnchantmentsCreation
                    mEnchCreation.SetItemEnchantments(player,
                                                      New Item _
                                                         With {.Id = inventoryItem.Id,
                                                         .Socket1Effectid = inventoryItem.Socket1Effectid,
                                                         .Socket2Effectid = inventoryItem.Socket2Effectid,
                                                         .Socket3Effectid = inventoryItem.Socket3Effectid,
                                                         .EnchantmentEffectid = inventoryItem.EnchantmentEffectid},
                                                      TryInt(newguid), GlobalVariables.targetCore,
                                                      GlobalVariables.targetStructure)
                Next
            End If
        End Sub

        Private Sub CreateAtMangos(ByVal characterguid As Integer, ByVal player As Character)
            LogAppend("Adding inventory at mangos", "InventoryCreation_createAtMangos", False)
            Dim bagexist As List(Of String) = New List(Of String)
            Dim bagstring As String = ""
            Const bagEnchString As String =
                      "0 1191182336 3 0 1065353216 0 1 0 1 0 0 0 0 0 1 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 3753 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 100 100 0 0 "
            Dim enchString As String =
                    "0 1191182336 3 0 1065353216 0 1 0 1 0 0 0 0 0 1 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 3753 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 100 100 0 0 "
            If GlobalVariables.targetExpansion < 3 Then _
                enchString =
                    "0 1191182336 3 0 1065353216 0 1 0 1 0 0 0 0 0 1 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 3753 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 100 100 0 0 "

            bagexist.Clear()
            If Not player.InventoryZeroItems Is Nothing Then
                For Each inventoryItem As Item In player.InventoryZeroItems
                    Dim newguid As String =
                            ((TryInt(
                                runSQLCommand_characters_string(
                                    "SELECT " & GlobalVariables.targetStructure.itmins_guid_col(0) & " FROM " &
                                    GlobalVariables.targetStructure.item_instance_tbl(0) & " WHERE " &
                                    GlobalVariables.targetStructure.itmins_guid_col(0) &
                                    "=(SELECT MAX(" & GlobalVariables.targetStructure.itmins_guid_col(0) & ") FROM " &
                                    GlobalVariables.targetStructure.item_instance_tbl(0) & ")")) + 1)).ToString

                    Dim parts() As String = bagEnchString.Split(" "c)
                    parts(0) = newguid
                    parts(3) = inventoryItem.Id.ToString()
                    parts(14) = "1"
                    Dim myEnchString As String = String.Join(" ", parts)

                    runSQLCommand_characters_string(
                        "INSERT INTO " & GlobalVariables.targetStructure.item_instance_tbl(0) & " ( " &
                        GlobalVariables.targetStructure.itmins_guid_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_ownerGuid_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_data_col(0) &
                        " ) VALUES ( '" &
                        newguid & "', '" & characterguid.ToString() &
                        "', '" & myEnchString & "' )")

                    runSQLCommand_characters_string(
                        "INSERT INTO " & GlobalVariables.targetStructure.character_inventory_tbl(0) & " ( " &
                        GlobalVariables.targetStructure.invent_guid_col(0) & ", " &
                        GlobalVariables.targetStructure.invent_bag_col(0) & ", `" &
                        GlobalVariables.targetStructure.invent_slot_col(0) & "`, `" &
                        GlobalVariables.targetStructure.invent_item_col(0) & "`, `" &
                        GlobalVariables.targetStructure.invent_item_template_col(0) &
                        "` ) VALUES ( '" &
                        characterguid.ToString() & "', '" &
                        inventoryItem.Bag.ToString() & "', '" &
                        inventoryItem.Slot.ToString() & "', '" &
                        newguid & "', '" &
                        inventoryItem.Id.ToString() & "')")
                    Select Case inventoryItem.Slot
                        Case 19, 20, 21, 22, 67, 68, 69, 70, 71, 72, 73
                            '// Item is a bag and has to be registered
                            bagstring = bagstring & "oldguid:" & inventoryItem.Guid.ToString() & ";newguid:" & newguid &
                                        ";"
                    End Select
                    Dim mEnchCreation As New EnchantmentsCreation
                    mEnchCreation.SetItemEnchantments(player,
                                                      New Item _
                                                         With {.Id = inventoryItem.Id,
                                                         .Socket1Effectid = inventoryItem.Socket1Effectid,
                                                         .Socket2Effectid = inventoryItem.Socket2Effectid,
                                                         .Socket3Effectid = inventoryItem.Socket3Effectid,
                                                         .EnchantmentEffectid = inventoryItem.EnchantmentEffectid},
                                                      TryInt(newguid), GlobalVariables.targetCore,
                                                      GlobalVariables.targetStructure)
                Next
            End If
            If Not player.InventoryItems Is Nothing Then
                For Each inventoryItem As Item In player.InventoryItems
                    Dim newguid As String =
                            ((TryInt(
                                runSQLCommand_characters_string(
                                    "SELECT " & GlobalVariables.targetStructure.itmins_guid_col(0) & " FROM " &
                                    GlobalVariables.targetStructure.item_instance_tbl(0) & " WHERE " &
                                    GlobalVariables.targetStructure.itmins_guid_col(0) &
                                    "=(SELECT MAX(" & GlobalVariables.targetStructure.itmins_guid_col(0) & ") FROM " &
                                    GlobalVariables.targetStructure.item_instance_tbl(0) & ")")) + 1)).ToString
                    Dim newbagguid As String = SplitString(bagstring,
                                                           "oldguid:" & inventoryItem.Bagguid.ToString() & ";newguid:",
                                                           ";")

                    Dim parts() As String = enchString.Split(" "c)
                    parts(0) = newguid
                    parts(3) = inventoryItem.Id.ToString()
                    parts(14) = "1"
                    Dim myEnchString As String = String.Join(" ", parts)

                    runSQLCommand_characters_string(
                        "INSERT INTO " & GlobalVariables.targetStructure.item_instance_tbl(0) & " ( " &
                        GlobalVariables.targetStructure.itmins_guid_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_ownerGuid_col(0) & ", " &
                        GlobalVariables.targetStructure.itmins_data_col(0) & " ) VALUES ( '" &
                        newguid & "', '" &
                        characterguid.ToString() & "', '" &
                        myEnchString & "' )")
                    runSQLCommand_characters_string(
                        "INSERT INTO " & GlobalVariables.targetStructure.character_inventory_tbl(0) & " ( " &
                        GlobalVariables.targetStructure.invent_guid_col(0) & ", " &
                        GlobalVariables.targetStructure.invent_bag_col(0) & ", `" &
                        GlobalVariables.targetStructure.invent_slot_col(0) & "`, `" &
                        GlobalVariables.targetStructure.invent_item_col(0) & "`, `" &
                        GlobalVariables.targetStructure.invent_item_template_col(0) & "` ) VALUES ( '" &
                        characterguid.ToString() & "', '" &
                        newbagguid & "', '" &
                        inventoryItem.Slot.ToString() & "', '" &
                        newguid & "', '" &
                        inventoryItem.Id.ToString() & "')")
                    Dim mEnchCreation As New EnchantmentsCreation
                    mEnchCreation.SetItemEnchantments(player,
                                                      New Item _
                                                         With {.Id = inventoryItem.Id,
                                                         .Socket1Effectid = inventoryItem.Socket1Effectid,
                                                         .Socket2Effectid = inventoryItem.Socket2Effectid,
                                                         .Socket3Effectid = inventoryItem.Socket3Effectid,
                                                         .EnchantmentEffectid = inventoryItem.EnchantmentEffectid},
                                                      TryInt(newguid), GlobalVariables.targetCore,
                                                      GlobalVariables.targetStructure)
                Next
            End If
        End Sub
    End Class
End Namespace