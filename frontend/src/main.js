import './assets/main.css'
import { createApp } from 'vue'
import App from './App.vue'
import mitt from 'mitt';
import axios from 'axios';

import 'vuetify/styles'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'

const vuetify = createVuetify({
  components,
  directives,
  theme: {
    dark: true
  }
})

const app = createApp(App).use(vuetify);
const emitter = mitt();

app.config.globalProperties.emitter = emitter;

const url = 'http://localhost:5001/init';
function initDatabase() {
  const r = axios.get(url);
  r.then(x => {
    console.log(x.data);
    if (x.data < 0) {
      setTimeout(initDatabase, 10000);
      alert('Initializing database, may take up to a few minutes. Be patient.');
    } else
      app.mount('#app');
  });
}

initDatabase();