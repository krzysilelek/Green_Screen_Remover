.data
; maska do liczenia greenscrena B * (-0.5) + G * 1.0 + R * (-0.5) + A * 0
greenMask dd -0.5, 1.0, -0.5, 0.0 
.code
processPictureAsm proc
start:
        ; rdx - punkt pocz�tkowy, r8 - punkt ko�cowy
        ; rcx - tablica bajt�w (obraz do edycji), xmm3 - pr�g podany przez u�ytkownika 
        
        ; je�eli rdx i r8 s� takie same to skocz do ending (koniec wykonywania procedury)
        cmp rdx, r8 
        je ending

        ; przekazanie sk�adowych piksela do xmm0
        pmovzxbd xmm0, [rcx + rdx]

        ; konwersja zawarto�ci xmm0 (sk�adowe piksela)
        cvtdq2ps xmm0, xmm0
        
        ; przekazanie maski (drugi wektor w obliczeniach) do xmm1
        movups xmm1, [greenMask] 
        
        ; wykonanie iloczynu skalarnego dla danych w xmm0 i xmm1
        ; wynik dzia�ania zostanie zapisany do najni�szej cz�ci xmm0
        dpps xmm0, xmm1, 241

        ; por�wnanie zawarto�ci xmm0 (wynik iloczynu skalarnego) z xmm3 (pr�g ustawiony przez u�ytkownika)
        comiss xmm0, xmm3
        
        ; je�eli xmm0 jest wi�ksze ni� xmm3 to wykonywany jest skok do changeToZero
        ja changeToZero

        ; przeskok na kolejny piksel i powr�t na pocz�tek
        add rdx, 4
        jmp start

changeToZero:
        ; wyzerowanie kana�u Alpha dla danego piksela
        mov BYTE PTR [rcx + rdx + 3], 0
        
        ; przeskok na kolejny piksel i powr�t na pocz�tek
        add rdx, 4
        jmp start
ending: 
        ret
processPictureAsm endp
end