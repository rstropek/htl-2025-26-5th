## Adding Delete Button to DataGrid

```
<DataGridTemplateColumn Header="" Width="Auto">
    <DataGridTemplateColumn.CellTemplate>
        <DataTemplate>
            <Button Content="Delete" 
                    Command="{Binding $parent[Window].DataContext.DeleteRecordingJobCommand}" 
                    CommandParameter="{Binding}"/>
        </DataTemplate>
    </DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>
```
