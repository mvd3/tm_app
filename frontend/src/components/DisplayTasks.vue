<template>
    <div class="container">
        <div class="filterContainer">
            <div style="min-width: 80px;">
                <v-btn v-if="currentPage > 0" variant="outlined" @click="previousPage()">
                    Previous
                </v-btn>                
            </div>
            <v-responsive class="mx-auto" max-width="500" min-width="400">
                <v-text-field v-model="searchTerm" label="Search term" variant="solo-inverted" density="compact"></v-text-field>
            </v-responsive>
            <v-responsive class="mx-auto" max-width="400" min-width="300">
                <v-combobox v-model="selectedStatus"
                    label="Status"
                    :items="statusListComboBox"
                    variant="solo-inverted"
                    density="compact"
                ></v-combobox>
            </v-responsive>
            <div>
                <v-btn variant="outlined" @click="search()">
                    Search
                </v-btn>
            </div>
            <div style="min-width: 80px; margin-left: 30px">
                <v-btn v-if="currentPage + 1 < pageNumber" variant="outlined" @click="nextPage()">
                    Next
                </v-btn>
            </div>
        </div>
        <div class="infoContainer">
            <p v-if="totalNumberOfTasks > 0" class="green">
                Tasks: {{ totalNumberOfTasks }} (Showing from {{ currentPage*20 + 1 }} to {{ (currentPage+1)*20 < totalNumberOfTasks ? (currentPage+1)*20 : totalNumberOfTasks }})
            </p>
        </div>
        <div>
            <div v-if="tasksList.length > 0"
                class="tasksContainer">
                <v-expansion-panels>
                    <v-expansion-panel v-for="(item, index) in tasksList" :key="index">
                        <v-expansion-panel-title>
                            <v-row no-gutters>
                                <v-col cols="10" class="d-flex justify-start">
                                    <p v-if="item.StatusId != showDoneValue">
                                        {{ item.Name }} ({{ new Date(item.CreatedDate).toDateString() }})
                                    </p>
                                    <p v-if="item.StatusId == showDoneValue">
                                        <b>{{ item.Name }}</b> ({{ new Date(item.CreatedDate).toDateString() }})
                                    </p>
                                </v-col>
                                <v-col cols="1" class="d-flex justify-start">
                                    <v-btn v-if="item.StatusId == showDoneValue" variant="outlined"
                                        @click.native.stop="setTaskAsDone(item.Id)">
                                        Done
                                    </v-btn>
                                </v-col>
                                <v-col cols="1" class="d-flex justify-start">
                                    <v-btn variant="outlined" @click.native.stop="deleteTask(item.Id)">
                                        Delete
                                    </v-btn>
                                </v-col>
                            </v-row>
                        </v-expansion-panel-title>
                        <v-expansion-panel-text>
                            {{ item.Description }}
                        </v-expansion-panel-text>
                    </v-expansion-panel>
                </v-expansion-panels>
            </div>
            <div v-if="tasksList.length == 0"
                class="noData">
                <p class="green">NO TASKS</p>
            </div>
        </div>
    </div>
</template>

<style scoped>
    .noData {
        width: 100%;
        height: 250px;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 30px;
    }

    .tasksContainer {
        overflow-y: scroll;
    }

    .container {
        width: 100%;
        display: flex;
        flex-direction: column;
        gap: 20px;
    }

    .filterContainer {
        width: 100%;
        height: 70px;
        display: flex;
        flex-direction: row;
        border-bottom: 1px solid hsla(160, 100%, 37%, 1);
        justify-content: space-evenly;
        align-items: center;
        padding-bottom: 20px;
    }

    .infoContainer {
        width: 100%;
        display: flex;
        justify-content: center;
    }
</style>

<script>
import axios from 'axios';
export default {
    data() {
        return {
            tasksList: [],
            statusListComboBox: ['All', 'In progress', 'Done'],
            showDoneValue: 1,
            currentPage: 0,
            pageNumber: 1, //Total number of pages
            totalNumberOfTasks: 0,
            searchTerm: '',
            selectedStatus: 'All',
            currentSearchTerm: '',
            currentSelectedStatus: -1
        }
    },
    mounted() {
        this.currentPage = 0;
        this.loadTasks();
        this.loadTasksNumber();
        this.emitter.on('task-added', (x) => {
            if (this.currentSelectedStatus != 2)
                this.reloadList();
        });
    },
    methods: {
        loadTasks(status = -1, searchTerm = '', page = 0) {
            const data = {
                Status: status,
                SearchTerm: searchTerm,
                Page: page
            }
            const url = `http://localhost:5001/getTasks/${encodeURIComponent(JSON.stringify(data))}`;
            const r = axios.get(url);
            r.then(x => {
                this.tasksList = x.data;
            });
        },
        setTaskAsDone(id) {
            const url = `http://localhost:5001/setTaskAsDone/${encodeURIComponent(id)}`;
            const r = axios.get(url);
            r.then(x => {
                this.reloadList();
            });
        },
        deleteTask(id) {
            const url = `http://localhost:5001/deleteTask/${encodeURIComponent(id)}`;
            const r = axios.get(url);
            r.then(x => {
                this.reloadList();
            });
        },
        loadTasksNumber(status = -1, searchTerm = '') {
            const data = {
                Status: status,
                SearchTerm: searchTerm,
                Page: 0
            }
            const url = `http://localhost:5001/getNumberOfTasks/${encodeURIComponent(JSON.stringify(data))}`;
            const r = axios.get(url);
            r.then(x => {
                this.totalNumberOfTasks = x.data;
                this.pageNumber = this.totalNumberOfTasks % 20 == 0 
                    ? this.totalNumberOfTasks/20 
                    : (this.totalNumberOfTasks - this.totalNumberOfTasks % 20) + 1;
            });
        },
        search() {
            this.currentSearchTerm = '';
            if (this.searchTerm != null)
                if (this.searchTerm.length > 0)
                    this.currentSearchTerm = this.searchTerm;
            
            if (this.selectedStatus == this.statusListComboBox[1])
                this.currentSelectedStatus = 1;
            else if (this.selectedStatus == this.statusListComboBox[2])
                this.currentSelectedStatus = 2;
            else
                this.currentSelectedStatus = -1;

            this.currentPage = 0;
            
            this.loadTasks(this.currentSelectedStatus, this.currentSearchTerm, this.currentPage);
            this.loadTasksNumber(this.currentSelectedStatus, this.currentSearchTerm);
        },
        reloadList() {
            this.loadTasks(this.currentSelectedStatus, this.currentSearchTerm, this.currentPage);
            this.loadTasksNumber(this.currentSelectedStatus, this.currentSearchTerm);
        },
        nextPage() {
            this.currentPage++;
            this.reloadList();
        },
        previousPage() {
            this.currentPage--;
            this.reloadList();
        }
    }
}
</script>