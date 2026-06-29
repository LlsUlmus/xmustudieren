使用方法：
<sider-modal v-model:open="v" :closable="false" sider-title="补充资料" v-model:active-key="activeKey">
    <form-view title="基本信息" icon="credit-card-outlined" >
        123
    </form-view>
    <sub-view title="老师信息" icon="user-outlined" extra-icon="warning">
        456
    </sub-view>
    <sub-view title="学生信息" icon="user-outlined" extra-icon="success">
        789
    </sub-view>
</sider-modal>

sider-modal API：
props:
  open: 是否显示
  closable：右上角显示关闭图标，点击窗口外可以关闭
  width：宽度，默认900px
  siderTitle：对话框的标题，显示在左边
  errorMsg：仅当state为error时生效，错误提示语句
  activeKey：当前激活的面板
  extra-icon: 显示在菜单栏左边的图标。填写loading时，是一个转圈圈的图标，其它情况按填写内容
              此字段对 success, warning, error和loading 有特别处理

emits:
  update:activeKey：当面板切换时引起该事件。所以可以通过v-model:activeKey双向绑定key
  update:open：当面板显示状态切换时引起该事件。所以可以通过v-model:open双向绑定显示状态

form-view API：
props:
  icon：显示在左边列表的图标
  title：显示在左边列表的名称，也做key使用。所以不能重复
  其它属性：全部传递给props内部的form使用

emits:
  submit: 点击确认按钮时的事件，参数 { formRef, close, loading }，formRef是form的引用，close函数是关闭对话框用的，loading是确认按钮的读取状态
  cancel：点击取消按钮时的事件，参数同上。默认状态即是直接关闭对话框

sub-view API：
props:
  icon：显示在左边列表的图标
  title：显示在左边列表的名称，也做key使用。所以不能重复