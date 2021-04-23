--行为树基础逻辑&定义
BehavitorTree = {
    FSM = {},
    CFG = {},
}

BehavitorTree.NodeType = {
    Inverter = 0,      --逆变
    Succeeder = 1,     --成功
    Repeater = 2,      --重复
    RepeatUntilFail = 3,       --重复直至失败

    Parallel = 100,        --并行
    Sequence = 101,        --序列
    Selector = 102,        --选择
    Random = 103,          --随机

    Leaf = 200,            --叶节点
}

BehavitorTree.Result = {
    Running = 0,
    Successs = 1,
    Failed = 2,
}

--行为节点
local BTNode = BaseClass("BTNode")

BTNode.ctor = function(self)
    self.nodeType = 0
    self.childList = {}
    self.actionFunc = ""
    self.actionParam = ""
    self.func = nil
    self.done = false
end

BTNode.Reset = function(self)
    for i=1, #self.childList do
        self.childList[i]:Reset()
    end
    self.done = false
    self.func = nil
end

BTNode.Update = function(self, owner, dt)
    if self.nodeType == BehavitorTree.NodeType.Inverter then
        local result = self.childList[1]:Update(owner, dt)
        if result == BehavitorTree.Result.Successs then
            self.done = true
            return BehavitorTree.Result.Failed
        elseif result == BehavitorTree.Result.Failed then
            self.done = true
            return BehavitorTree.Result.Successs
        else
            return BehavitorTree.Result.Running
        end
    elseif self.nodeType == BehavitorTree.NodeType.Succeeder then
        local result = self.childList[1]:Update(owner, dt)
        self.done = true
        return result
    elseif self.nodeType == BehavitorTree.NodeType.Repeater then
        local result = self.childList[1]:Update(owner, dt)
        if result ~= BehavitorTree.Result.Running then
            self:Reset() 
        end
        return BehavitorTree.Result.Running
    elseif self.nodeType == BehavitorTree.NodeType.RepeatUntilFail then
        local result = self.childList[1]:Update(owner, dt)
        if result == BehavitorTree.Result.Failed then
            self.done = true
            return BehavitorTree.Result.Failed
        else
            if result == BehavitorTree.Result.Successs then
                self:Reset() 
            end
            return BehavitorTree.Result.Running
        end
    elseif self.nodeType == BehavitorTree.NodeType.Parallel then
        local running = false
        for i=1, #self.childList do
            if self.childList[i].done == false then
                local result = self.childList[i]:Update(owner, dt)
                if result == BehavitorTree.Result.Failed then
                    self.done = true
                    return BehavitorTree.Result.Failed
                elseif result == BehavitorTree.Result.Running then
                    running = true
                end
            end
        end
        if running then
            return BehavitorTree.Result.Running
        else
            self.done = true
            return BehavitorTree.Result.Successs
        end
    elseif self.nodeType == BehavitorTree.NodeType.Sequence then
        for i=1, #self.childList do
            if self.childList[i].done == false then
                local result = self.childList[i]:Update(owner, dt)
                if result == BehavitorTree.Result.Failed then
                    self.done = true
                    return BehavitorTree.Result.Failed
                else
                    return BehavitorTree.Result.Running
                end
            end
        end
        self.done = true
        return BehavitorTree.Result.Successs
    elseif self.nodeType == BehavitorTree.NodeType.Selector then
        for i=1, #self.childList do
            local result = self.childList[i]:Update(owner, dt)
            if result == BehavitorTree.Result.Successs then
                self.done = true
                return BehavitorTree.Result.Successs
            else
                return BehavitorTree.Result.Running
            end
        end
        self.done = true
        return BehavitorTree.Result.Failed
    elseif self.nodeType == BehavitorTree.NodeType.Random then
        local rd = math.random(1, #self.childList)
        local result = self.childList[rd]:Update(owner, dt)
        self.done = true
        return result
    elseif self.nodeType == BehavitorTree.NodeType.Leaf then
        if not self.func then
            self.func = BehavitorTree.FSM[self.actionFunc].New(self)
            self.func:OnEnter(owner)
        end
        local result = self.func:OnExec(owner)
        if result ~= BehavitorTree.Result.Running then
            self.func:OnExit(owner)
            self.done = true
        end
        return result
    end
    error("behavitor tree not run here")
end

return BTNode
