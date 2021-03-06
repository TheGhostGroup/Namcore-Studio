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
'*      /Filename:      ArmoryHandler
'*      /Description:   Contains functions for parsing character information from wow armory
'+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
Imports System.Threading
Imports NCFramework.Framework.Functions
Imports NCFramework.Framework.Modules
Imports NCFramework.Framework.Logging
Imports NCFramework.Framework.Extension
Imports System.Net
Imports System.Text
Imports NCFramework.Framework.Armory.Parser

Namespace Framework.Armory
    Public Class ArmoryHandler
        '// Declaration
        Private ReadOnly _context As SynchronizationContext = SynchronizationContext.Current
        Public Event Completed As EventHandler(Of CompletedEventArgs)
        '// Declaration

        Protected Overridable Sub OnCompleted(ByVal e As CompletedEventArgs)
            RaiseEvent Completed(Me, e)
        End Sub

        Public Sub LoadArmoryCharacters(ByVal urllst As List(Of String))
            ThreadExtensions.QueueUserWorkItem(New Func(Of List(Of String), String)(AddressOf DoLoad), urllst)
        End Sub

        Public Function DoLoad(ByVal linkList As List(Of String)) As String
            LogAppend("Loading characters from Armory (" & LinkList.Count.ToString() & " character/s)",
                      "ArmoryHandler_DoLoad", True)
            Dim setId As Integer = 0
            Dim characterContext As String
            Dim apiLink As String
            Dim characterName As String
            Dim client As New WebClient
            client.CheckProxy()
            Dim armoryAccount As New Account() With {.Name = "Armory", .Id = 0}
            armoryAccount.Characters = New List(Of Character)()
            armoryAccount.SetIndex = 0
            armoryAccount.SourceExpansion = 5
            armoryAccount.IsArmory = True
            armoryAccount.Core = Modules.Core.ARMORY
            AddAccountSet(0, armoryAccount)
            For Each armoryLink As String In linkList
                Try
                    LogAppend("URL is " & armoryLink, "ArmoryHandler_DoLoad", False)
                    characterContext = client.DownloadString(armoryLink)
                    Dim b() As Byte = Encoding.Default.GetBytes(characterContext)
                    characterContext = Encoding.UTF8.GetString(b)
                Catch ex As Exception
                    LogAppend("Failed to load character context. Exception is: " & vbNewLine & ex.ToString(),
                              "ArmoryHandler_DoLoad", True, True)
                    Continue For
                End Try
                Try
                    Dim realm As String = SplitString(armoryLink, "/character/", "/")
                    characterContext = characterContext.Replace("&#39;", "")
                    characterName = SplitString(armoryLink, "/" & realm & "/", "/")
                    apiLink = "http://" & SplitString(armoryLink, "http://", ".battle") &
                              ".battle.net/api/wow/character/" &
                              SplitString(armoryLink, "/character/", "/") & "/" & characterName
                    Dim apiContext As String = client.DownloadString(apiLink)
                    setId += 1
                    LogAppend("Loading character " & characterName & " //ident is " & setId.ToString(),
                              "ArmoryHandler_DoLoad", True)
                    LogAppend("Loading basic character information", "ArmoryHandler_DoLoad", True)
                    Dim player As New Character()
                    player.Name = characterName
                    player.Guid = 0
                    player.AccountId = 0
                    player.AccountName = "Armory"
                    player.Level = TryInt(SplitString(apiContext, """level"":", ","))
                    player.Gender(0) = TryUInt(SplitString(apiContext, """gender"":", ","))
                    player.Race(0) = TryUInt(SplitString(apiContext, """race"":", ","))
                    player.Cclass(0) = TryUInt(SplitString(apiContext, """class"":", ","))
                    player.SourceCore = Modules.Core.ARMORY
                    player.SourceExpansion = Expansion.MOP
                    player.LoadedDateTime = DateTime.Now
                    player.InventoryItems = New List(Of Item)()
                    player.InventoryZeroItems = New List(Of Item)()
                    player.PlayerGlyphs = New List(Of Glyph)()
                    player.PlayerReputation = New List(Of Reputation)()
                    player.Achievements = New List(Of Achievement)()
                    player.ArmorItems = New List(Of Item)()
                    player.Actions = New List(Of Action)()
                    player.Quests = New List(Of Quest)()
                    player.Spells = New List(Of Spell)()
                    player.Skills = New List(Of Skill)()
                    '// Character appearance
                    Try
                        LogAppend("Loading character appearance information", "ArmoryHandler_DoLoad", True)
                        Dim appearanceContext As String = client.DownloadString(apiLink & "?fields=appearance")
                        Dim appFace As Integer = CInt(Hex$(Long.Parse(SplitString(appearanceContext,
                                                                                  """faceVariation"":", ","))))
                        Dim appSkin As Integer = CInt(Hex$(Long.Parse(SplitString(appearanceContext, """skinColor"":",
                                                                                  ","))))
                        Dim appHairStyle As Integer = CInt(Hex$(Long.Parse(SplitString(appearanceContext,
                                                                                       """hairVariation"":",
                                                                                       ","))))
                        Dim appHairColor As Integer = CInt(Hex$(Long.Parse(SplitString(appearanceContext,
                                                                                       """hairColor"":",
                                                                                       ","))))
                        Dim appFeatureVar As Integer = CInt(Hex$(Long.Parse(SplitString(appearanceContext,
                                                                                        """featureVariation"":", ","))))
                        player.SetPlayerBytes(appSkin, appFace, appHairStyle, appHairColor)
                        player.SetPlayerBytes2(appFeatureVar)
                    Catch ex As Exception
                        LogAppend("Exception occured: " & vbNewLine & ex.ToString(), "ArmoryHandler_DoLoad", False, True)
                    End Try
                    LogAppend("Loading character's finished quests", "ArmoryHandler_DoLoad", True)
                    player.FinishedQuests = SplitString(client.DownloadString(apiLink & "?fields=quests") & ",",
                                                        """quests"":[", "]}")
                    player.InventoryZeroItems = New List(Of Item)()
                    player.InventoryZeroItems.Add(
                        New Item _
                                                     With {.Id = 6948, .Count = 1, .Bag = 0, .Container = 0, .Slot = 23,
                                                     .Guid = 0}) '// Adding hearthstone
                    player.SetIndex = setId
                    AddCharacterSet(setId, player, armoryAccount)
                    Dim mReputationParser As ReputationParser = New ReputationParser
                    mReputationParser.LoadReputation(setId, apiLink, armoryAccount)
                    Dim mGlyphParser As GlyphParser = New GlyphParser
                    mGlyphParser.LoadGlyphs(setId, apiLink, armoryAccount)
                    Dim mAchievementParser As AchievementParser = New AchievementParser
                    mAchievementParser.LoadAchievements(setId, apiLink, armoryAccount)
                    Dim mProfessionParser As ProfessionParser = New ProfessionParser
                    mProfessionParser.LoadProfessions(setId, apiLink, armoryAccount)
                    Dim mItemParser As ItemParser = New ItemParser
                    mItemParser.LoadItems(setId, apiLink, armoryAccount)
                    player.Loaded = True
                    SetCharacterSet(setId, player, armoryAccount)
                    SetAccountSet(armoryAccount.SetIndex, armoryAccount)
                    LogAppend("Character loaded!", "ArmoryHandler_DoLoad", True)
                Catch ex As Exception
                    LogAppend("Exception during character loading!: " & ex.ToString(), "ArmoryHandler_DoLoad", True,
                              True)
                End Try
            Next
            LogAppend("All characters loaded!", "ArmoryHandler_DoLoad", True)
            ThreadExtensions.ScSend(_context, New Action(Of CompletedEventArgs)(AddressOf OnCompleted),
                                    New CompletedEventArgs())
            Return ""
        End Function
    End Class
End Namespace