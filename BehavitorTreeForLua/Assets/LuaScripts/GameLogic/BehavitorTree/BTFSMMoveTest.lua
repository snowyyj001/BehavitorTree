local BTFsm = require "GameLogic.BehavitorTree.Base.BTFsm"
local BTFSMMoveTest = BaseClass("BTFSMMoveTest", BTFsm)

BTFSMMoveTest.ctor = function(self, btnode)
    BTFSMMoveTest.super.ctor(self, btnode)
end

BTFSMMoveTest.OnEnter = function(self, owner)
    print("BTFSMMoveTest.OnEnter")
    owner.m_Animator:Play ("move")
    owner:SetMovePath(g_Map.movePaths[tonumber(self.btnode.actionParam)])
end

BTFSMMoveTest.OnExec = function(self, owner)
    if owner:IsDead() then
        return BehavitorTree.Result.Failed
    end
    if #owner.movePath == 0 then
        return BehavitorTree.Result.Successs
    end
    return BehavitorTree.Result.Running
end

BTFSMMoveTest.OnExit = function(self, owner)
    print("BTFSMMoveTest.OnExit")
end

return BTFSMMoveTest