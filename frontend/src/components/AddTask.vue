<template>
    <h2 class="green">Add Task</h2>
    <hr class="green" />
    <form @submit.prevent="addTask" 
        style="margin-top: 15px;">
        <div style="padding-top: 5px;">
            <v-responsive class="mx-auto" max-width="500" min-width="400">
                <v-text-field v-model="newTask.name" label="Name" variant="solo-inverted"></v-text-field>
            </v-responsive>
        </div>
        <div>
            <v-responsive class="mx-auto" max-width="500" min-width="400">
                <v-textarea v-model="newTask.description" auto-grow label="Description" variant="solo-inverted"></v-textarea>
            </v-responsive>
        </div>
        <v-btn type="submit" variant="outlined">
            Add
        </v-btn>
    </form>
</template>

<script>
import axios from 'axios';

export default {
    data() {
        return {
            newTask: {
                name: '',
                description: ''
            }
        };
    },
    methods: {
        addTask() {
            if (this.newTask.name.length > 0)
                this.postTask(this.newTask.name, this.newTask.description);
            this.newTask.name = '';
            this.newTask.description = '';
        },
        postTask(name, description) {
            const data = {
                Id: 0,
                Name: name,
                Description: description,
                StatusId: 1
            }
            const url = `http://localhost:5001/addTask/${encodeURIComponent(JSON.stringify(data))}`;
            const r = axios.get(url);
            r.then(x => {
                this.emitter.emit('task-added', 0);
            });
        }
    }
};
</script>