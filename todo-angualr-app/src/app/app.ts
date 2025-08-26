import { Component, Injectable, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';


interface Todo {
  id: number;
  title: string;
  isComplete: boolean;
  position: number;
}

@Injectable()
@Component({
  selector: 'app-root',
  //imports: [RouterOutlet],
    imports: [CommonModule, RouterOutlet, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App  implements OnInit{
    todos: Todo[] = [];
     newTodoTitle = '';
  protected readonly title = signal('todo-angualr-app');

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
     this.loadTodos();
  }

    loadTodos() {
    this.http.get<Todo[]>('api/Todo').subscribe({
      next: (result) => {
        console.log('Todos loaded', result);
        // Ensure sorted by position
        this.todos = (result || []).sort((a, b) => a.position - b.position);
      },
      error: err => console.error('Failed to load todos', err)
    });
  }

    addTodo() {
    const title = (this.newTodoTitle || '').trim();
    if (!title) return;
    const payload = { title, isComplete: false, position: 0 } as Partial<Todo>;
    this.http.post<Todo>('api/Todo', payload).subscribe({
      next: () => { this.newTodoTitle = ''; this.loadTodos(); },
      error: err => console.error('Failed to add todo', err)
    });
  }

toggleComplete(t: Todo) {
    const updated = { ...t, isComplete: !t.isComplete };
    this.http.put(`api/Todo/${t.id}`, updated).subscribe({
      next: () => this.loadTodos(),
      error: err => console.error('Failed to update todo', err)
    });
  }

    deleteTodo(id: number) {
    this.http.delete(`api/Todo/${id}`).subscribe({
      next: () => this.loadTodos(),
      error: err => console.error('Failed to delete todo', err)
    });
  }
 
    moveUp(id: number) {
    this.http.post(`api/Todo/move-up/${id}`, {}).subscribe({
      next: () => this.loadTodos(),
      error: err => console.error('Failed to move up', err)
    });
  }
  moveDown(id: number) {
    this.http.post(`api/Todo/move-down/${id}`, {}).subscribe({
      next: () => this.loadTodos(),
      error: err => console.error('Failed to move down', err)
    });
  }

}
