<script lang="ts">
import { ref, onMounted } from 'vue';

interface TodoDto {
  id: number;
  title: string;
  isComplete: boolean;
  position: number;
}

export default {
      name: 'TodoList',
       setup() {
    const API_BASE = 'api/Todo';
    const todos = ref<TodoDto[]>([]);
    const loading = ref(true);
    const error = ref<string | null>(null);
    const newTitle = ref('');

    async function load() {
      loading.value = true;
      error.value = null;
      try {
        const res = await fetch(`${API_BASE}`);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        todos.value = await res.json();
      } catch (err: any) {
        error.value = err?.message ?? String(err);
      } finally {
        loading.value = false;
      }
    }

    async function addTodo() {
      if (!newTitle.value.trim()) return;
      const todo = { title: newTitle.value.trim(), isComplete: false, position: 0 };
      const res = await fetch(`${API_BASE}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(todo),
      });
      if (res.ok) {
        // Prefer reloading to keep positions consistent
        await load();
        newTitle.value = '';
      } else {
        error.value = `Add failed: ${res.status}`;
      }
    }

    async function deleteTodo(id: number) {
      const res = await fetch(`${API_BASE}/${id}`, { method: 'DELETE' });
      if (res.ok) {
        await load();
      } else {
        error.value = `Delete failed: ${res.status}`;
      }
    }

    async function moveUp(id: number) {
      const res = await fetch(`${API_BASE}/move-up/${id}`, { method: 'POST' });
      if (res.ok) await load(); else error.value = `Move up failed: ${res.status}`;
    }

    async function moveDown(id: number) {
      const res = await fetch(`${API_BASE}/move-down/${id}`, { method: 'POST' });
      if (res.ok) await load(); else error.value = `Move down failed: ${res.status}`;
    }

    async function toggleComplete(todo: TodoDto) {
      const res = await fetch(`${API_BASE}/${todo.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...todo, isComplete: !todo.isComplete }),
      });
      if (res.ok) await load(); else error.value = `Update failed: ${res.status}`;
    }

    onMounted(() => {
      load();
    });

    return { todos, loading, error, newTitle, addTodo, deleteTodo, moveUp, moveDown, toggleComplete, load };
  }

  }
    </script>

    <template>
  <section class="todo">
    <header class="header">
      <h2>My Tasks</h2>
      <div class="controls">
        <input v-model="newTitle" placeholder="Add a new task and press Enter" @keyup.enter="addTodo" aria-label="New task title" />
        <button class="btn primary" @click="addTodo" title="Add task">Add</button>
        <button class="btn ghost" @click="load" title="Refresh list">Refresh</button>
      </div>
    </header>

    <div v-if="loading" class="loading">Loading tasks…</div>
    <div v-if="error" class="error">Error: {{ error }}</div>

    <transition-group name="list" tag="ul" class="list">
      <li v-for="todo in todos" :key="todo.id" :class="['item', { complete: todo.isComplete }]">
        <div class="left">
          <!-- <div class="position">{{ todo.position }}</div> -->
          <input class="chk" type="checkbox" :checked="todo.isComplete" @change="() => toggleComplete(todo)" :aria-checked="todo.isComplete" :aria-label="`Mark ${todo.title} complete`" />
          <div class="meta">
            <div class="title">{{ todo.title }}</div>
            <div class="subtitle">#{{ todo.id }} • {{ todo.isComplete ? 'Done' : 'Pending' }}</div>
          </div>
        </div>

        <div class="actions">
          <button class="icon" @click="() => moveUp(todo.id)" title="Move up">▲</button>
          <button class="icon" @click="() => moveDown(todo.id)" title="Move down">▼</button>
          <button class="danger" @click="() => deleteTodo(todo.id)" title="Delete task">Delete</button>
        </div>
      </li>
    </transition-group>
  </section>
</template>

<style scoped>
.todo { max-width: 700px; margin: 1rem auto; }
.add { display:flex; gap:8px; margin-bottom:12px; }
ul { list-style:none; padding:0; }
/* Modern card list styles */
.todo { max-width:800px; margin:1.75rem auto; background:linear-gradient(180deg,#fff,#fbfbff); padding:18px; border-radius:12px; box-shadow:0 8px 30px rgba(18,25,40,0.06); }
.header { display:flex; align-items:center; justify-content:space-between; gap:12px; margin-bottom:12px }
.header h2 { margin:0; font-size:1.4rem; letter-spacing:-0.4px }
.controls { display:flex; gap:8px; align-items:center }
.controls input { padding:10px 12px; border-radius:10px; border:1px solid #e6e9ef; min-width:260px; outline:none; background:#fff }
.controls input:focus { box-shadow:0 6px 18px rgba(76,132,255,0.12); border-color:#4c84ff }
.btn { padding:8px 12px; border-radius:10px; border:0; cursor:pointer; font-weight:600 }
.btn.primary { background:linear-gradient(90deg,#5b8cff,#4c6bff); color:white }
.btn.ghost { background:transparent; border:1px solid #e6e9ef }
.loading { padding:12px; color:#555 }
.error { color:#bf1650; padding:8px 0 }
.list { list-style:none; margin:0; padding:0 }
.list .item { display:flex; align-items:center; justify-content:space-between; gap:12px; padding:12px; margin:8px 0; border-radius:10px; background:linear-gradient(180deg,#ffffff,#fcfdff); box-shadow:0 6px 18px rgba(19,27,44,0.03); transition:transform .18s ease, box-shadow .18s ease }
.list .item:hover { transform:translateY(-4px); box-shadow:0 18px 36px rgba(19,27,44,0.06) }
.list .item.complete { opacity:0.78 }
.left { display:flex; align-items:center; gap:12px }
.position { width:44px; height:44px; display:flex; align-items:center; justify-content:center; border-radius:10px; background:linear-gradient(180deg,#eef2ff,#f8fbff); font-weight:700; color:#3750ff }
.chk { width:18px; height:18px }
.meta .title { font-size:1rem; font-weight:600 }
.meta .subtitle { font-size:0.8rem; color:#748095 }
.actions { display:flex; gap:8px; align-items:center }
.icon { background:transparent; border:1px solid #e6e9ef; padding:6px 8px; border-radius:8px; cursor:pointer }
.icon:hover { background:#f4f7ff }
.danger { background:transparent; border:0; color:#d23; font-weight:700; cursor:pointer }

/* Transition-group animations */
.list-move { transition: transform .25s ease }

</style>