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
'*      /Filename:      Live_View
'*      /Description:   Main interface with following functions:
'*                      -List all accounts and characters
'*                      -Editing/Deleting/Transferring accounts and characters
'+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

Imports Namcore_Studio.ConnectionHandler
Imports Namcore_Studio.GlobalVariables
Imports Namcore_Studio.Account_CharacterInformationProcessing
Imports Namcore_Studio.CommandHandler
Imports Namcore_Studio.Conversions
Imports System.Resources

Public Class Live_View
    Private cmpFileListViewComparer As ListViewComparer
    Dim checkchangestatus As Boolean = False
    Dim target_accchar_table As DataTable
    Private Sub connect_bt_Click(sender As System.Object, e As System.EventArgs) Handles connect_bt.Click
        con_operator = 1
        DB_connect.Show()
    End Sub
    Public Sub loadaccountsandchars()
        checkchangestatus = False
        sourceCore = "trinity" 'for testing only
        acctable = returnAccountTable(GlobalConnection_Realm)
        chartable = returnCharacterTable(GlobalConnection)
        modifiedAccTable = acctable.Copy
        modifiedCharTable = chartable.Copy
        characterview.Items.Clear()
        accountview.Items.Clear()
        For Each rowitem As DataRow In acctable.Rows
            Dim str(4) As String
            Dim itm As ListViewItem
            str(0) = rowitem.Item(0)
            str(1) = rowitem.Item(1)
            str(2) = rowitem.Item(2)
            str(3) = rowitem.Item(3)
            str(4) = rowitem.Item(4)
            itm = New ListViewItem(str)
            accountview.Items.Add(itm)
            accountview.EnsureVisible(accountview.Items.Count - 1)
        Next
        accountview.Update()
        For Each rowitem As DataRow In chartable.Rows
            Dim str(6) As String
            Dim itm As ListViewItem
            str(0) = rowitem.Item(0)
            str(1) = rowitem.Item(1)
            str(2) = rowitem.Item(2)
            str(3) = GetRaceNameById(TryInt(rowitem.Item(3)))
            str(4) = GetClassNameById(TryInt(rowitem.Item(4)))
            str(5) = GetGenderNameById(TryInt(rowitem.Item(5)))
            str(6) = rowitem.Item(6)
            itm = New ListViewItem(str)
            characterview.Items.Add(itm)
            characterview.EnsureVisible(characterview.Items.Count - 1)
        Next
        characterview.Sort()
        characterview.Update()
        checkchangestatus = True
        acctotal.Text = "(" & accountview.Items.Count.ToString() & " Accounts total)"
        chartotal.Text = "(" & characterview.Items.Count.ToString() & " Characters total)"
    End Sub
    Public Sub loadtargetaccountsandchars()
        targetCore = "trinity" 'for testing only
        target_accchar_table = returnTargetAccCharTable(TargetConnection_Realm)
        target_accounts_tree.Nodes.Clear()
        For Each rowitem As DataRow In target_accchar_table.Rows
            Dim foundNode() As TreeNode = target_accounts_tree.Nodes.Find(rowitem(0), False)
            If foundNode.Length = 0 Then
                Dim newnode As New TreeNode
                With newnode
                    .Name = rowitem.Item(0)
                    .Text = rowitem.Item(1)
                End With
                target_accounts_tree.Nodes.Add(newnode)
            Else
                Dim Node As TreeNode = target_accounts_tree.Nodes.Find(rowitem(0), False)(0)
                Dim SubNode As New TreeNode
                With SubNode
                    .Name = rowitem(2)
                    .Text = rowitem(3)
                End With

                Node.Nodes.Add(SubNode)
            End If
        Next
        target_accounts_tree.Update()
    End Sub
    Public Sub setaccountview(ByVal accounttable As DataTable)
        checkchangestatus = False
        sourceCore = "trinity" 'for testing only

        characterview.Items.Clear()
        accountview.Items.Clear()
        For Each rowitem As DataRow In accounttable.Rows
            Dim str(4) As String
            Dim itm As ListViewItem
            str(0) = rowitem.Item(0)
            str(1) = rowitem.Item(1)
            str(2) = rowitem.Item(2)
            str(3) = rowitem.Item(3)
            str(4) = rowitem.Item(4)
            itm = New ListViewItem(str)
            accountview.Items.Add(itm)
            accountview.EnsureVisible(accountview.Items.Count - 1)
        Next
        accountview.Update()
        For Each accrowitem As DataRow In accounttable.Rows
            Dim accid As String = accrowitem.Item(0)
            For Each rowitem As DataRow In chartable.Rows
                If rowitem(1) = accid Then
                    Dim str(6) As String
                    Dim itm As ListViewItem
                    str(0) = rowitem.Item(0)
                    str(1) = rowitem.Item(1)
                    str(2) = rowitem.Item(2)
                    str(3) = GetRaceNameById(TryInt(rowitem.Item(3)))
                    str(4) = GetClassNameById(TryInt(rowitem.Item(4)))
                    str(5) = GetGenderNameById(TryInt(rowitem.Item(5)))
                    str(6) = rowitem.Item(6)
                    itm = New ListViewItem(str)
                    characterview.Items.Add(itm)
                    characterview.EnsureVisible(characterview.Items.Count - 1)
                End If
            Next
        Next
        characterview.Update()
        checkchangestatus = True
        acctotal.Text = "(" & accountview.Items.Count.ToString() & " Accounts total)"
        chartotal.Text = "(" & characterview.Items.Count.ToString() & " Characters total)"
    End Sub
    Public Sub setcharacterview(ByVal charactertable As DataTable)
        checkchangestatus = False
        sourceCore = "trinity" 'for testing only
        characterview.Items.Clear()
        accountview.Items.Clear()
        For Each rowitem As DataRow In modifiedAccTable.Rows
            Dim str(4) As String
            Dim itm As ListViewItem
            str(0) = rowitem.Item(0)
            str(1) = rowitem.Item(1)
            str(2) = rowitem.Item(2)
            str(3) = rowitem.Item(3)
            str(4) = rowitem.Item(4)
            itm = New ListViewItem(str)
            accountview.Items.Add(itm)
            accountview.EnsureVisible(accountview.Items.Count - 1)
        Next
        accountview.Update()
        For Each accrowitem As DataRow In modifiedAccTable.Rows
            Dim accid As String = accrowitem.Item(0)
            For Each rowitem As DataRow In charactertable.Rows
                If rowitem(1) = accid Then
                    Dim str(6) As String
                    Dim itm As ListViewItem
                    str(0) = rowitem.Item(0)
                    str(1) = rowitem.Item(1)
                    str(2) = rowitem.Item(2)
                    str(3) = GetRaceNameById(TryInt(rowitem.Item(3)))
                    str(4) = GetClassNameById(TryInt(rowitem.Item(4)))
                    str(5) = GetGenderNameById(TryInt(rowitem.Item(5)))
                    str(6) = rowitem.Item(6)
                    itm = New ListViewItem(str)
                    characterview.Items.Add(itm)
                    characterview.EnsureVisible(characterview.Items.Count - 1)
                End If
            Next
        Next
        characterview.Update()
        checkchangestatus = True
        acctotal.Text = "(" & accountview.Items.Count.ToString() & " Accounts total)"
        chartotal.Text = "(" & characterview.Items.Count.ToString() & " Characters total)"
    End Sub
    Private Sub Live_View_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load

    End Sub

    Private Sub accountview_ColumnClick(sender As Object, e As System.Windows.Forms.ColumnClickEventArgs) Handles accountview.ColumnClick
        If e.Column = cmpFileListViewComparer.SortColumn Then
            If cmpFileListViewComparer.SortOrder = SortOrder.Ascending Then
                cmpFileListViewComparer.SortOrder = SortOrder.Descending
            Else
                cmpFileListViewComparer.SortOrder = SortOrder.Ascending
            End If
        Else
            cmpFileListViewComparer.SortOrder = SortOrder.Ascending
        End If

        cmpFileListViewComparer.SortColumn = e.Column
        accountview.Sort()
    End Sub


    Private Sub accountview_ItemChecked1(sender As Object, e As System.Windows.Forms.ItemCheckedEventArgs) Handles accountview.ItemChecked
        If checkchangestatus = True Then
            If Not accountview.CheckedItems.Count = 0 Then
                characterview.Items.Clear()


                For Each checkedrow As ListViewItem In accountview.CheckedItems
                    Dim accid As String = checkedrow.SubItems(0).Text
                    For Each rowitem As DataRow In modifiedCharTable.Rows
                        If rowitem(1) = accid Then
                            Dim str(6) As String
                            Dim itm As ListViewItem
                            str(0) = rowitem.Item(0)
                            str(1) = rowitem.Item(1)
                            str(2) = rowitem.Item(2)
                            str(3) = GetRaceNameById(TryInt(rowitem.Item(3)))
                            str(4) = GetClassNameById(TryInt(rowitem.Item(4)))
                            str(5) = GetGenderNameById(TryInt(rowitem.Item(5)))
                            str(6) = rowitem.Item(6)
                            itm = New ListViewItem(str)
                            characterview.Items.Add(itm)
                            characterview.EnsureVisible(characterview.Items.Count - 1)
                        End If
                    Next
                Next
                characterview.Update()
                For Each listitem As ListViewItem In characterview.Items
                    listitem.Checked = True
                Next
            Else
                characterview.Items.Clear()
                For Each listitems As ListViewItem In accountview.Items
                    Dim accid As String = listitems.SubItems(0).Text
                    For Each rowitem As DataRow In modifiedCharTable.Rows
                        If rowitem(1) = accid Then
                            Dim str(6) As String
                            Dim itm As ListViewItem
                            str(0) = rowitem.Item(0)
                            str(1) = rowitem.Item(1)
                            str(2) = rowitem.Item(2)
                            str(3) = GetRaceNameById(TryInt(rowitem.Item(3)))
                            str(4) = GetClassNameById(TryInt(rowitem.Item(4)))
                            str(5) = GetGenderNameById(TryInt(rowitem.Item(5)))
                            str(6) = rowitem.Item(6)
                            itm = New ListViewItem(str)
                            characterview.Items.Add(itm)
                            characterview.EnsureVisible(characterview.Items.Count - 1)
                        End If
                    Next
                Next
                characterview.Update()
            End If
        End If
        acctotal.Text = "(" & accountview.Items.Count.ToString() & " Accounts total)"
        chartotal.Text = "(" & characterview.Items.Count.ToString() & " Characters total)"
    End Sub

    Private Sub accountview_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles accountview.SelectedIndexChanged

    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As System.Object, e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles checkall_acc.LinkClicked
        For Each xitem As ListViewItem In accountview.Items
            xitem.Checked = True
        Next
    End Sub

    Private Sub uncheckall_acc_LinkClicked(sender As System.Object, e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles uncheckall_acc.LinkClicked
        For Each xitem As ListViewItem In accountview.Items
            xitem.Checked = False
        Next
    End Sub



    Private Sub filter_acc_LinkClicked(sender As System.Object, e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles filter_acc.LinkClicked
        Filter_accounts.Show()
    End Sub

    Private Sub SelectedAccountsToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles SelectedAccountsToolStripMenuItem.Click
        Dim RM As New ResourceManager("Namcore_Studio.UserMessages", System.Reflection.Assembly.GetExecutingAssembly())
        Dim result = MsgBox(RM.GetString("deleteacc") & " (" & accountview.SelectedItems(0).SubItems(1).Text & ")", vbYesNo, RM.GetString("areyousure"))
        If result = Microsoft.VisualBasic.MsgBoxResult.Yes Then
            Dim accountId As String = accountview.SelectedItems(0).SubItems(0).Text
            For I = 0 To accountview.SelectedItems.Count - 1
                accountview.SelectedItems(I).Remove()
                GlobalVariables.acc_id_columnname = "id" 'todo
                Dim toBeRemovedRow As DataRow() = acctable.Select(GlobalVariables.acc_id_columnname & " = '" & accountId & "'")
                If Not toBeRemovedRow.Length = 0 Then acctable.Rows.Remove(toBeRemovedRow(0))
                runSQLCommand_realm_string_setconn("DELETE FROM `" & account_tablename & "` WHERE " & acc_id_columnname & "='" & accountId & "'", GlobalConnection_Realm)
                runSQLCommand_characters_string_setconn("DELETE FROM `" & character_tablename & "` WHERE " & char_accountId_columnname & "='" & accountId & "'", GlobalConnection)
            Next
            setaccountview(acctable)
        End If
    End Sub

    Private Sub CheckedAccountsToolStripMenuItem1_Click(sender As System.Object, e As System.EventArgs) Handles CheckedAccountsToolStripMenuItem1.Click
        Dim RM As New ResourceManager("Namcore_Studio.UserMessages", System.Reflection.Assembly.GetExecutingAssembly())
        Dim result = MsgBox(RM.GetString("deleteacc"), vbYesNo, RM.GetString("areyousure"))
        If result = Microsoft.VisualBasic.MsgBoxResult.Yes Then
            For Each itm As ListViewItem In accountview.CheckedItems
                accountview.Items.Remove(itm)
                GlobalVariables.acc_id_columnname = "id" 'todo
                Dim toBeRemovedRow As DataRow() = acctable.Select(GlobalVariables.acc_id_columnname & " = '" & itm.SubItems(0).Text & "'")
                If Not toBeRemovedRow.Length = 0 Then acctable.Rows.Remove(toBeRemovedRow(0))
                runSQLCommand_realm_string_setconn("DELETE FROM `" & account_tablename & "` WHERE " & acc_id_columnname & "='" & itm.SubItems(0).Text & "'", GlobalConnection_Realm)
                runSQLCommand_characters_string_setconn("DELETE FROM `" & character_tablename & "` WHERE " & char_accountId_columnname & "='" & itm.SubItems(0).Text & "'", GlobalConnection)
            Next
            setaccountview(acctable)
        End If
    End Sub

    Private Sub EditToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles EditToolStripMenuItem.Click
        'todo
    End Sub
    Private Sub accountview_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles accountview.MouseDown
        If e.Button = MouseButtons.Right Then
            Dim oItem As ListViewItem = accountview.GetItemAt(e.X, e.Y)
            If oItem IsNot Nothing Then
                For I = 0 To accountview.SelectedItems.Count - 1
                    accountcontext.Show(accountview, e.X, e.Y)
                Next
            End If
        End If
    End Sub

    Private Sub filter_char_LinkClicked(sender As System.Object, e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles filter_char.LinkClicked
        Filter_characters.Show()
    End Sub


    Private Sub getlogin_bt_Click(sender As System.Object, e As System.EventArgs)

    End Sub

    Private Sub connect_bt_target_Click(sender As System.Object, e As System.EventArgs) Handles connect_bt_target.Click
        con_operator = 2
        DB_connect.Show()
    End Sub

    Private Sub accountcontext_Opening(sender As System.Object, e As System.ComponentModel.CancelEventArgs) Handles accountcontext.Opening

    End Sub

    Private Sub RemoveToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles RemoveToolStripMenuItem.Click

    End Sub

    Private Sub SelectedCharacterToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles SelectedCharacterToolStripMenuItem.Click
        Dim RM As New ResourceManager("Namcore_Studio.UserMessages", System.Reflection.Assembly.GetExecutingAssembly())
        Dim result = MsgBox(RM.GetString("deletechar") & " (" & characterview.SelectedItems(0).SubItems(2).Text & ")", vbYesNo, RM.GetString("areyousure"))
        If result = Microsoft.VisualBasic.MsgBoxResult.Yes Then
            Dim charId As String = characterview.SelectedItems(0).SubItems(0).Text
            For I = 0 To characterview.SelectedItems.Count - 1
                characterview.SelectedItems(I).Remove()
                Dim toBeRemovedRow As DataRow() = chartable.Select(char_guid_columnname & " = '" & charId & "'")
                If Not toBeRemovedRow.Length = 0 Then chartable.Rows.Remove(toBeRemovedRow(0))
                runSQLCommand_characters_string_setconn("DELETE FROM `" & character_tablename & "` WHERE " & char_guid_columnname & "='" & charId & "'", GlobalConnection)
            Next
            setaccountview(acctable)
        End If
    End Sub

    Private Sub CheckedCharactersToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles CheckedCharactersToolStripMenuItem.Click
        Dim RM As New ResourceManager("Namcore_Studio.UserMessages", System.Reflection.Assembly.GetExecutingAssembly())
        Dim result = MsgBox(RM.GetString("deletechar"), vbYesNo, RM.GetString("areyousure"))
        If result = Microsoft.VisualBasic.MsgBoxResult.Yes Then
            For Each itm As ListViewItem In characterview.CheckedItems
                characterview.Items.Remove(itm)
                Dim toBeRemovedRow As DataRow() = chartable.Select(char_guid_columnname & " = '" & itm.SubItems(0).Text & "'")
                If Not toBeRemovedRow.Length = 0 Then chartable.Rows.Remove(toBeRemovedRow(0))
                runSQLCommand_characters_string_setconn("DELETE FROM `" & character_tablename & "` WHERE " & char_guid_columnname & "='" & itm.SubItems(0).Text & "'", GlobalConnection)
            Next
            setaccountview(acctable)
        End If
    End Sub

    Private Sub targetacccontext_Opening(sender As System.Object, e As System.ComponentModel.CancelEventArgs) Handles targetacccontext.Opening

    End Sub

    Private Sub characterview_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles characterview.MouseDown
        If e.Button = MouseButtons.Right Then
            Dim oItem As ListViewItem = characterview.GetItemAt(e.X, e.Y)
            If oItem IsNot Nothing Then
                For I = 0 To characterview.SelectedItems.Count - 1
                    charactercontext.Show(characterview, e.X, e.Y)
                Next
            End If
        End If
    End Sub

    Private Sub characterview_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles characterview.SelectedIndexChanged

    End Sub

    Private Sub target_accounts_tree_AfterSelect(sender As System.Object, e As System.Windows.Forms.TreeViewEventArgs) Handles target_accounts_tree.AfterSelect

    End Sub

    Private Sub target_accounts_tree_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles target_accounts_tree.MouseDown
        If e.Button = MouseButtons.Right Then
            Dim oItem As TreeNode = target_accounts_tree.GetNodeAt(e.X, e.Y)
            If oItem IsNot Nothing Then
                If oItem.Level = 0 Then
                    targetacccontext.Show(target_accounts_tree, e.X, e.Y)
                Else
                    targetcharcontext.Show(target_accounts_tree, e.X, e.Y)
                End If

            End If
        End If
    End Sub

    Private Sub RemoveToolStripMenuItem2_Click(sender As System.Object, e As System.EventArgs) Handles RemoveToolStripMenuItem2.Click
        Dim RM As New ResourceManager("Namcore_Studio.UserMessages", System.Reflection.Assembly.GetExecutingAssembly())
        Dim result = MsgBox(RM.GetString("deleteacc") & " (" & target_accounts_tree.SelectedNode.Text & ")", vbYesNo, RM.GetString("areyousure"))
        If result = Microsoft.VisualBasic.MsgBoxResult.Yes Then
            Dim accountId As String = target_accounts_tree.SelectedNode.Name
            target_accounts_tree.SelectedNode.Remove()
            GlobalVariables.acc_id_columnname = "id" 'todo
            Dim toBeRemovedRow As DataRow() = target_accchar_table.Select(GlobalVariables.acc_id_columnname & " = '" & accountId & "'")
            If Not toBeRemovedRow.Length = 0 Then target_accchar_table.Rows.Remove(toBeRemovedRow(0))
            'runSQLCommand_realm_string_setconn("DELETE FROM `" & account_tablename & "` WHERE " & acc_id_columnname & "='" & accountId & "'", TargetConnection_Realm)
            'runSQLCommand_characters_string_setconn("DELETE FROM `" & character_tablename & "` WHERE " & char_accountId_columnname & "='" & accountId & "'", TargetConnection)
        End If
    End Sub

    Private Sub ToolStripMenuItem1_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripMenuItem1.Click
        Dim RM As New ResourceManager("Namcore_Studio.UserMessages", System.Reflection.Assembly.GetExecutingAssembly())
        Dim result = MsgBox(RM.GetString("deleteacc") & " (" & target_accounts_tree.SelectedNode.Text & ")", vbYesNo, RM.GetString("areyousure"))
        If result = Microsoft.VisualBasic.MsgBoxResult.Yes Then
            Dim accountId As String = target_accounts_tree.SelectedNode.Name
            target_accounts_tree.SelectedNode.Remove()
            GlobalVariables.acc_id_columnname = "id" 'todo
            Dim toBeRemovedRow As DataRow() = target_accchar_table.Select(GlobalVariables.acc_id_columnname & " = '" & accountId & "'")
            If Not toBeRemovedRow.Length = 0 Then target_accchar_table.Rows.Remove(toBeRemovedRow(0))
            'runSQLCommand_characters_string_setconn("DELETE FROM `" & character_tablename & "` WHERE " & char_accountId_columnname & "='" & accountId & "'", TargetConnection)
        End If
    End Sub
End Class
