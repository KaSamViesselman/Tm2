Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class AuditTableFields
    Inherits System.Web.UI.Page
    Private _currentUser As KaUser

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        If Not Utilities.GetUserPagePermission(_currentUser, New List(Of String)({"reports"}), "Reports")("reports").Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            PopulateTables()
            ddlTable_SelectedIndexChanged(ddlTable, New EventArgs())
        End If
        lblStatus.Text = ""
    End Sub

    Private Sub PopulateTables()
        With ddlTable.Items
            .Clear()
            .Add(New ListItem("Select table", ""))
            Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT name FROM sys.tables WHERE (NOT (name LIKE '%ticket%')) AND (name <> 'audit') ORDER BY name")
            Do While rdr.Read
                .Add(New ListItem(rdr.Item("name"), rdr.Item("name")))
            Loop
        End With
        ddlTable.SelectedIndex = 0
    End Sub

    Private Sub ddlTable_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlTable.SelectedIndexChanged
        Do While tblTableFields.Rows.Count > 1
            tblTableFields.Rows.RemoveAt(1)
        Loop
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim auditFields As New List(Of KaAuditField)
        If ddlTable.SelectedIndex > 0 Then
            Dim invalidAuditFields As New List(Of String) From {KaRecord.FN_CREATED, KaRecord.FN_ID, KaRecord.FN_LAST_UPDATED, KaRecord.FN_LAST_UPDATED_APPLICATION, KaRecord.FN_LAST_UPDATED_USER}
            auditFields.Clear()
            Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, String.Format("SELECT af.id, c.name AS column_name " & _
                                                        "FROM sys.tables AS t " & _
                                                        "INNER JOIN sys.columns AS c ON t.object_id = c.object_id " & _
                                                        "LEFT OUTER JOIN (SELECT * FROM audit_fields WHERE (deleted = 0)) AS af ON t.name = af.table_name AND c.name = af.field_name " & _
                                                        "WHERE (t.name = {0}) " & _
                                                        "ORDER BY column_name", Q(ddlTable.SelectedValue)))

            Do While rdr.Read
                Dim columnName As String = rdr.Item("column_name")
                If invalidAuditFields.Contains(columnName) Then Continue Do
                Dim auditFieldId As Guid = Guid.NewGuid
                auditFieldId = IsNull(rdr.Item("id"), auditFieldId)
                Dim auditField As KaAuditField
                Try
                    auditField = New KaAuditField(connection, auditFieldId)
                Catch ex As RecordNotFoundException
                    auditField = New KaAuditField()
                    auditField.Id = Guid.NewGuid
                    auditField.TableName = ddlTable.SelectedValue
                    auditField.FieldName = columnName
                End Try
                auditFields.Add(auditField)
            Loop
        End If
        CreateDynamicControls(auditFields)
    End Sub

    Private Sub btnSave_Click(sender As Object, e As System.EventArgs) Handles btnSave.Click
        If ddlTable.SelectedIndex > 0 Then
            Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
            Dim validIds As New List(Of Guid)
            Dim auditFields As List(Of KaAuditField) = ConvertAuditTableToObjects()
            For Each auditField As KaAuditField In auditFields
                If auditField.AuditWhenInserted OrElse auditField.AuditWhenUpdated OrElse auditField.AuditWhenDeleted Then
                    auditField.SqlUpdateInsertIfNotFound(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    validIds.Add(auditField.Id)
                End If
            Next

            Dim validIdString As String = ""
            For Each id As Guid In validIds
                If validIdString.Length > 0 Then validIdString &= ","
                validIdString &= Q(id)
            Next

            Tm2Database.ExecuteNonQuery(connection, String.Format("UPDATE {0} SET {1} = 1, {2} = {3}, {4} = {5} WHERE {6} = {7}{8}", KaAuditField.TABLE_NAME, KaAuditField.FN_DELETED, KaAuditField.FN_LAST_UPDATED_APPLICATION, Q(Database.ApplicationIdentifier), KaAuditField.FN_LAST_UPDATED_USER, Q(_currentUser.Name), KaAuditField.FN_TABLE_NAME, Q(ddlTable.SelectedValue), IIf(validIdString.Length > 0, " AND NOT id IN (" & validIdString & ")", "")))

            Dim trigger As SqlTrigger = KaAudit.GetFieldAuditTrigger(ddlTable.SelectedValue)
            DropTrigger(connection, trigger.Name)
            If validIds.Count > 0 Then CreateTrigger(connection, trigger)
            ddlTable_SelectedIndexChanged(ddlTable, New EventArgs())
            lblStatus.Text = "Table saved successfully"
        End If
    End Sub

    Private Function ConvertAuditTableToObjects() As List(Of KaAuditField)
        Dim auditFields As New List(Of KaAuditField)
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        auditFields.Clear()
        For rowCounter As Integer = 1 To tblTableFields.Rows.Count - 1 ' Skip the first header row
            Dim fieldRow As HtmlTableRow = tblTableFields.Rows(rowCounter)

            Dim auditFieldId As Guid = Guid.Empty
            If Guid.TryParse(fieldRow.ID, auditFieldId) Then
                Dim auditField As KaAuditField
                Try
                    auditField = New KaAuditField(connection, auditFieldId)
                Catch ex As RecordNotFoundException
                    auditField = New KaAuditField()
                    auditField.Id = auditFieldId
                End Try
                auditField.TableName = ddlTable.SelectedValue
                auditField.FieldName = CType(tblTableFields.FindControl(fieldRow.ID & "_name"), HtmlTableCell).InnerText
                auditField.AuditWhenInserted = CType(tblTableFields.FindControl(fieldRow.ID & "_insertAudit"), CheckBox).Checked
                auditField.AuditWhenUpdated = CType(tblTableFields.FindControl(fieldRow.ID & "_updateAudit"), CheckBox).Checked
                auditField.AuditWhenDeleted = CType(tblTableFields.FindControl(fieldRow.ID & "_deleteAudit"), CheckBox).Checked
                auditFields.Add(auditField)
            End If
        Next
        Return auditFields
    End Function

    Private Sub CreateDynamicControls(auditFields As List(Of KaAuditField))
        For Each auditField As KaAuditField In auditFields
            Dim fieldRow As New HtmlTableRow
            With fieldRow
                .ID = auditField.Id.ToString
                .EnableViewState = True
            End With
            tblTableFields.Rows.Add(fieldRow)
            Dim fieldNameCell As New HtmlTableCell
            With fieldNameCell
                .ID = fieldRow.ID & "_name"
                .InnerText = auditField.FieldName
                .EnableViewState = True
            End With
            fieldRow.Cells.Add(fieldNameCell)

            Dim fieldInsertAuditCell As New HtmlTableCell
            fieldRow.Cells.Add(fieldInsertAuditCell)
            Dim fieldInsertAuditCheckbox As New CheckBox
            With fieldInsertAuditCheckbox
                .ID = fieldRow.ID & "_insertAudit"
                .Text = ""
                .Checked = auditField.AuditWhenInserted
                .EnableViewState = True
            End With
            fieldInsertAuditCell.Controls.Add(fieldInsertAuditCheckbox)

            Dim fieldUpdateAuditCell As New HtmlTableCell
            fieldRow.Cells.Add(fieldUpdateAuditCell)
            Dim fieldUpdateAuditCheckbox As New CheckBox
            With fieldUpdateAuditCheckbox
                .ID = fieldRow.ID & "_updateAudit"
                .Text = ""
                .Checked = auditField.AuditWhenUpdated
                .EnableViewState = True
            End With
            fieldUpdateAuditCell.Controls.Add(fieldUpdateAuditCheckbox)

            Dim fieldDeleteAuditCell As New HtmlTableCell
            fieldRow.Cells.Add(fieldDeleteAuditCell)
            Dim fieldDeleteAuditCheckbox As New CheckBox
            With fieldDeleteAuditCheckbox
                .ID = fieldRow.ID & "_deleteAudit"
                .Text = ""
                .Checked = auditField.AuditWhenDeleted
                .EnableViewState = True
            End With
            fieldDeleteAuditCell.Controls.Add(fieldDeleteAuditCheckbox)
        Next
        tblTableFields.Visible = tblTableFields.Rows.Count > 1
        btnSave.Enabled = tblTableFields.Rows.Count > 1
    End Sub

    Protected Overrides Function SaveViewState() As Object
        Dim viewState(2) As Object
        'Saving the grid values to the View State
        Dim auditFields As List(Of KaAuditField) = ConvertAuditTableToObjects()
        viewState(0) = MyBase.SaveViewState()
        viewState(1) = auditFields
        Return viewState
    End Function

    Protected Overrides Sub LoadViewState(savedState As Object)
        'Getting the dropdown list value from view state.
        If savedState IsNot Nothing AndAlso CType(savedState, Object).Length > 1 Then
            Dim viewState As Object() = savedState
            Dim auditFields As List(Of KaAuditField) = viewState(1)
            CreateDynamicControls(auditFields)
            MyBase.LoadViewState(viewState(0))
        Else
            MyBase.LoadViewState(savedState)
        End If
    End Sub
End Class