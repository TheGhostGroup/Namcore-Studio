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
'*      /Filename:      SpellCreation
'*      /Description:   Includes functions for setting up the known spells of a specific
'*                      character
'+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
Imports NCFramework.Framework.Module
Imports NCFramework.Framework.Logging
Imports NCFramework.Framework.Database

Namespace Framework.Transmission
    Public Module SpellCreation
        Public Sub AddSpells(ByVal spellstring As String, ByVal player As Character, Optional forceTargetCore As Boolean = False)
            'TODO
            Dim useCore As String
            Dim useStructure As DbStructure
            If forceTargetCore Then
                useCore = GlobalVariables.targetCore
                useStructure = GlobalVariables.targetStructure
            Else
                useCore = GlobalVariables.sourceCore
                useStructure = GlobalVariables.sourceStructure
            End If
            Dim mySpells() As String = spellstring.Split(","c)
            Dim spellCount As Integer = UBound(Split(spellstring, ","))
            For i = 0 To spellCount - 1
                Dim mySpell As String = mySpells(i)
                LogAppend("Adding Spell " & mySpell, "SpellCreation_AddSpells")
                Select Case useCore
                    Case "trinity"
                        runSQLCommand_characters_string(
                            "INSERT INTO `" & useStructure.character_spells_tbl(0) & "`( `" &
                            useStructure.spell_guid_col(0) & "`, `" &
                            useStructure.spell_spell_col(0) & "`, `" &
                            useStructure.spell_active_col(0) & "`, `" &
                            useStructure.spell_disabled_col(0) &
                            "` ) VALUES ( '" &
                            player.CreatedGuid.ToString & "', '" &
                            mySpell & "', '1', '0' )", forceTargetCore)
                End Select
            Next
        End Sub

        Public Sub AddSpecialSpells(ByVal targetSetId As Integer, ByVal player As Character)
            'TODO
        End Sub
    End Module
End Namespace