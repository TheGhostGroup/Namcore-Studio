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
'*      /Filename:      PlayerCreateHelper
'*      /Description:   Provides functions to load correct spells/items for character 
'*                      races/classes
'+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
Imports NCFramework.Framework.Modules
Imports NCFramework.Framework.Functions
Namespace Framework.Transmission
    '// Declaration
    <Flags> Public Enum ChrRaces
        PLAYER_RACE_ALL = 0
        PLAYER_RACE_HUMAN = 1
        PLAYER_RACE_ORC = 2
        PLAYER_RACE_DWARF = 4
        PLAYER_RACE_NIGHT_ELF = 8
        PLAYER_RACE_UNDEAD = 16
        PLAYER_RACE_TAUREN = 32
        PLAYER_RACE_GNOME = 64
        PLAYER_RACE_TROLL = 128
        PLAYER_RACE_GOBLIN = 256
        PLAYER_RACE_BLOOD_ELF = 512
        PLAYER_RACE_DRAENEI = 1024
        PLAYER_RACE_FEL_ORC = 2048
        PLAYER_RACE_NAGA = 4096
        PLAYER_RACE_BROKEN = 8192
        PLAYER_RACE_SKELETON = 16384
        PLAYER_RACE_VRYKUL = 32768
        PLAYER_RACE_TUSKARR = 65536
        PLAYER_RACE_FOREST_TROLL = 131072
        PLAYER_RACE_TAUNKA = 262144
        PLAYER_RACE_NORTHREND_SKELETON = 524288
        PLAYER_RACE_ICE_TROLL = 1048576
        PLAYER_RACE_WORGEN = 2097152
    End Enum
    Public Enum ChrRaceIds
        PLAYER_RACE_ALL = 0
        PLAYER_RACE_HUMAN = 1
        PLAYER_RACE_ORC = 2
        PLAYER_RACE_DWARF = 3
        PLAYER_RACE_NIGHT_ELF = 4
        PLAYER_RACE_UNDEAD = 5
        PLAYER_RACE_TAUREN = 6
        PLAYER_RACE_GNOME = 7
        PLAYER_RACE_TROLL = 8
        PLAYER_RACE_GOBLIN = 9
        PLAYER_RACE_BLOOD_ELF = 10
        PLAYER_RACE_DRAENEI = 11
        PLAYER_RACE_FEL_ORC = 12
        PLAYER_RACE_NAGA = 13
        PLAYER_RACE_BROKEN = 14
        PLAYER_RACE_SKELETON = 15
        PLAYER_RACE_VRYKUL = 16
        PLAYER_RACE_TUSKARR = 17
        PLAYER_RACE_FOREST_TROLL = 18
        PLAYER_RACE_TAUNKA = 19
        PLAYER_RACE_NORTHREND_SKELETON = 20
        PLAYER_RACE_ICE_TROLL = 21
        PLAYER_RACE_WORGEN = 22
    End Enum
    <Flags> Public Enum ChrClasses
        PLAYER_CLASS_ALL = 0
        PLAYER_CLASS_WARRIOR = 1
        PLAYER_CLASS_PALADIN = 2
        PLAYER_CLASS_HUNTER = 4
        PLAYER_CLASS_ROGUE = 8
        PLAYER_CLASS_PRIEST = 16
        PLAYER_CLASS_DEATH_KNIGHT = 32
        PLAYER_CLASS_SHAMAN = 64
        PLAYER_CLASS_MAGE = 128
        PLAYER_CLASS_WARLOCK = 256
        PLAYER_CLASS_DRUID = 1024
    End Enum
    Public Enum ChrClassIds
        PLAYER_CLASS_ALL = 0
        PLAYER_CLASS_WARRIOR = 1
        PLAYER_CLASS_PALADIN = 2
        PLAYER_CLASS_HUNTER = 3
        PLAYER_CLASS_ROGUE = 4
        PLAYER_CLASS_PRIEST = 5
        PLAYER_CLASS_DEATH_KNIGHT = 6
        PLAYER_CLASS_SHAMAN = 7
        PLAYER_CLASS_MAGE = 8
        PLAYER_CLASS_WARLOCK = 9
        PLAYER_CLASS_DRUID = 11
    End Enum
    '// Declaration
    Public Module PlayerCreateHelper
        Public Sub GetRaceSpells(ByRef player As Character, ByVal account As Account)
            Dim thisRace As ChrRaceIds = player.Race
            Dim thisRaceBit As ChrRaces = thisRace
            Dim newSpellList As New List(Of Spell)
            Dim SpellsDt As DataTable
            For Each spellEntry In SpellsDt.Rows
                If TryInt(spellEntry(0)) = 0 Then
                    newSpellList.Add(New Spell With
                                     {.Active = 1, .Disabled = 0, .Id = TryInt(spellEntry(2)), .Name = spellEntry(3)})
                    Continue For
                End If
                Dim raceMask As ChrRaces = TryInt(spellEntry(0))
                If (raceMask And thisRaceBit) = thisRaceBit Then
                    newSpellList.Add(New Spell With
                                     {.Active = 1, .Disabled = 0, .Id = TryInt(spellEntry(2)), .Name = spellEntry(3)})
                End If
            Next
            player.Spells = newSpellList
            SetCharacterSet(player.SetIndex, player, account)
        End Sub
        Public Sub GetClassSpells(ByRef player As Character, ByVal account As Account)
            Dim thisClass As ChrClassIds = player.Cclass
            Dim thisClassBit As ChrClasses = thisClass
            Dim newSpellList As New List(Of Spell)
            Dim SpellsDt As DataTable
            For Each spellEntry In SpellsDt.Rows
                If TryInt(spellEntry(1)) = 0 Then
                    newSpellList.Add(New Spell With
                                     {.Active = 1, .Disabled = 0, .Id = TryInt(spellEntry(2)), .Name = spellEntry(3)})
                    Continue For
                End If
                Dim classMask As ChrClasses = TryInt(spellEntry(1))
                If (classMask And thisClassBit) = thisClassBit Then
                    newSpellList.Add(New Spell With
                                     {.Active = 1, .Disabled = 0, .Id = TryInt(spellEntry(2)), .Name = spellEntry(3)})
                End If
            Next
            player.Spells = newSpellList
            SetCharacterSet(player.SetIndex, player, account)
        End Sub
    End Module
End Namespace