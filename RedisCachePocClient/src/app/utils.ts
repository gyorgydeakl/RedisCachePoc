import {resource} from '@angular/core';
import {catchError, firstValueFrom, Observable} from 'rxjs';

export function resourceObs<TParam, TRes>(params: () => TParam, loader: (param: TParam) => Observable<TRes>) {
  return resource({
    params: params,
    loader: p => firstValueFrom(loader(p.params).pipe(catchError(e => {
      throw new Error("Failed to load resource");
    })))
  })
}


export function resourceObsNoParams<TRes>(loader: () => Observable<TRes>) {
  return resource({
    params: () => "",
    loader: _ => firstValueFrom(loader().pipe(catchError(e => {
      throw new Error("Failed to load resource");
    })))
  })
}
