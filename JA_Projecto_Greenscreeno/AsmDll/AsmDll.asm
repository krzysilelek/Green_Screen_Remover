.data
; maska do liczenia greenscrena B * (-0.5) + G * 1.0 + R * (-0.5) + A * 0
greenMask dd -0.5, 1.0, -0.5, 0.0 
.code
processPictureAsm proc
start:
        ; rdx - punkt pocz¹tkowy, r8 - punkt koñcowy
        ; rcx - tablica bajtów (obraz do edycji), xmm3 - próg podany przez u¿ytkownika 
        
        ; je¿eli rdx i r8 s¹ takie same to skocz do ending (koniec wykonywania procedury)
        cmp rdx, r8 
        je ending

        ; przekazanie sk³adowych piksela do xmm0
        pmovzxbd xmm0, [rcx + rdx]

        ; konwersja zawartoœci xmm0 (sk³adowe piksela)
        cvtdq2ps xmm0, xmm0
        
        ; przekazanie maski (drugi wektor w obliczeniach) do xmm1
        movups xmm1, [greenMask] 
        
        ; wykonanie iloczynu skalarnego dla danych w xmm0 i xmm1
        ; wynik dzia³ania zostanie zapisany do najni¿szej czêœci xmm0
        dpps xmm0, xmm1, 241

        ; porównanie zawartoœci xmm0 (wynik iloczynu skalarnego) z xmm3 (próg ustawiony przez u¿ytkownika)
        comiss xmm0, xmm3
        
        ; je¿eli xmm0 jest wiêksze ni¿ xmm3 to wykonywany jest skok do changeToZero
        ja changeToZero

        ; przeskok na kolejny piksel i powrót na pocz¹tek
        add rdx, 4
        jmp start

changeToZero:
        ; wyzerowanie kana³u Alpha dla danego piksela
        mov BYTE PTR [rcx + rdx + 3], 0
        
        ; przeskok na kolejny piksel i powrót na pocz¹tek
        add rdx, 4
        jmp start
ending: 
        ret
processPictureAsm endp
end