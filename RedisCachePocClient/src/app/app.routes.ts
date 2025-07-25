import { Routes } from '@angular/router';
import {MovieListComponent} from './reviewer/movie-list/movie-list.component';
import {MovieDetailsComponent} from './reviewer/movie-details/movie-details.component';
import {AddUserComponent} from './reviewer/add-user/add-user.component';
import {UserListComponent} from './planner/user-list/user-list.component';
import {WatchlistComponent} from './planner/watchlist/watchlist.component';

export const routes: Routes = [
  { path: 'reviewer/movies', component: MovieListComponent },
  { path: 'reviewer/movies/:movieId', component: MovieDetailsComponent },
  { path: 'reviewer/users/new', component: AddUserComponent },
  { path: 'planner/users', component: UserListComponent, },
  { path: 'planner/watchlist/:userId', component: WatchlistComponent, },
  { path: '**', redirectTo: 'reviewer/movies', pathMatch: 'full' }
];
