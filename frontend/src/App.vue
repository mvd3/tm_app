<script setup>
import AddTask from './components/AddTask.vue';
import DisplayTasks from './components/DisplayTasks.vue';
</script>

<template>
  <div class="container">
    <div class="header">
      <p class="green">Adacta Project - Tasks Management</p>
      <p class="green">|</p>
      <p class="green">Nikola Bubić</p>
    </div>
    <div class="content">
      <div class="tasks">
        <DisplayTasks>
        </DisplayTasks>
      </div>
      <div class="addTask">
        <AddTask>
        </AddTask>
      </div>
    </div>
  </div>
</template>

<style scoped>
.container {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  min-width: 1200px;
  min-height: 800px;
  overflow: scroll;
}
.header {
  width: 100%;
  height: 50px;
  border-bottom: 1px solid hsla(160, 100%, 37%, 1);
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: center;
  gap: 15px;
}

.header p {
  font-size: 25px;
}

.content {
  height: 100%;
  display: flex;
  flex-direction: row;
}

.tasks {
  height: 100%;
  flex-grow: 1;
  padding: 25px 15px;
}

.addTask {
  height: 100%;
  border-left: 1px solid hsla(160, 100%, 37%, 1);
  padding: 25px 15px;
}
</style>
