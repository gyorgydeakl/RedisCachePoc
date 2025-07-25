import { Routes } from '@angular/router';
import {MovieListComponent} from './movie-list/movie-list.component';
import {MovieDetailsComponent} from './movie-details/movie-details.component';
import {AddUserComponent} from './add-user/add-user.component';

export const routes: Routes = [
  { path: '', redirectTo: 'movie', pathMatch: 'full' },
  { path: 'movies', component: MovieListComponent },
  { path: 'movies/:movieId', component: MovieDetailsComponent },
  { path: 'users/new', component: AddUserComponent },
  { path: '**', redirectTo: 'movie', pathMatch: 'full' }
];
