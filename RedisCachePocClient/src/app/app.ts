import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {MovieListComponent} from './movie-list/movie-list.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, MovieListComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('RedisCachePocClient');
}
