<template>
    <sub-view v-bind="$attrs">
        <div class="sub-view-title">登录信息</div>
        <div class="list-items">
            <div class="list-item">
                <div class="item-meta">
                    <div class="item-avatar">
                        <a-icon icon="key-outlined" class="icon" />
                    </div>
                    <div class="item-content">
                        <h4 class="meta-title">密码登录</h4>
                        <div class="meta-description">
                            通过账号密码登录本系统。
                        </div>
                    </div>
                </div>
                <ul class="item-actions">
                    <li>
                        <edit-password url="/api/users/ResetPassword" />
                    </li>
                </ul>
            </div>
        </div>
    </sub-view>
</template>
<script setup>
import { ref, reactive, inject, watch } from 'vue'
import editPassword from './edit-password.vue';
</script>
