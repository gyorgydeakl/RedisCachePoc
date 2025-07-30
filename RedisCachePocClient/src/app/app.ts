import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {MovieListComponent} from './reviewer/movie-list/movie-list.component';
import {Toast} from 'primeng/toast';
import {AddMovieComponent} from './reviewer/add-movie/add-movie.component';
import {NavbarComponent} from './navbar/navbar.component';

@Component({
  selector: 'app-root',
  imports: [Toast, RouterOutlet, NavbarComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('RedisCachePocClient');
}
