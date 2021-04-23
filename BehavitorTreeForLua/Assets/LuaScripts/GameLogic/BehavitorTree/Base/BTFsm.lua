--叶节点状态机
local BTFsm = BaseClass("BTFsm")

BTFsm.ctor = function(self, btnode)
    self.btnode = btnode
end

BTFsm.OnEnter = function(self, owner)
end

BTFsm.OnExec = function(self, owner)
end

BTFsm.OnExit = function(self, owner)
end

return BTFsm