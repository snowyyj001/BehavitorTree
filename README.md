# BehavitorTree
a unity tool about BehavitorTree for lua

1.配置leaf动作列表
在Resources\BehaviorTree目录中actionlist.txt配置程序提前写好的可用行为动作
2.导入模板或者已有的行为树文件
模板路径在Resources\BehaviorTree\Lua中
3.编辑行为树后，点击save按钮保存lua文件到Assets\LuaScripts\BTAi中
4.在FSMDefine中配置导出的lua文件
5.使用BTManager.AddBtree BTManager.Start BTManager.Update使用行为树，其中owner是行为主体
